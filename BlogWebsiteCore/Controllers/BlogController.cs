using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogWebsiteCore.Data;
using BlogWebsiteCore.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using BlogWebsiteCore.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace BlogWebsiteCore.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private IHttpContextAccessor _httpContextAccessor;

        public BlogController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            HttpContextAccessorProp = httpContextAccessor;
        }

        public IHttpContextAccessor HttpContextAccessorProp 
        { 
            get
            {
                return _httpContextAccessor;
            }
            set
            {
                _httpContextAccessor = value;
            }
        }

        // GET: Blog
        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewBag.DateOldestSortParm = "date_oldest";

            var blogs = from b in _context.Blogs
                        select b;

            switch (sortOrder)
            {
                case "date_oldest":
                    blogs = blogs.OrderBy(b => b.CreatedDate);
                    break;
                default:
                    blogs = blogs.OrderByDescending(b => b.CreatedDate);
                    break;
            }

            return View(await blogs.ToListAsync());
        }

        // GET: Blog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            BlogCommentViewModel viewModel = new BlogCommentViewModel();
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(m => m.BlogId == id);
            if (blog == null)
            {
                return NotFound();
            }

            viewModel.Blog = blog;
            var comments = _context.Comments.Where(c => c.BlogId == id).ToList();
            viewModel.Comments = comments;
            await _context.SaveChangesAsync();
            
            return View(viewModel);
        }
        
        [HttpPost, ActionName("Details")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddComment([Bind("CommentId, Message")] Comment comment, int? id)
        {
            if (ModelState.IsValid)
            {
                comment.BlogId = (int)id;
                string userId = HttpContextAccessorProp
                                                .HttpContext
                                                .User
                                                .FindFirst(ClaimTypes.NameIdentifier)
                                                .Value;
                comment.AuthorId = userId;
                _context.Comments.Add(comment);

                var blog = await _context.Blogs
                    .FirstOrDefaultAsync(b => b.BlogId == id);
                blog.CountOfComments++;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details));
            }
            //ViewBag.BlogId = new SelectList(_context.Blogs, "BlogId", "BlogName", comment.BlogId);
            return RedirectToAction(nameof(Details));
        }

        // GET: Blog/Create
        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Blog/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BlogId,BlogName,Category,Content,ImageFile")] Blog blog)
        {
            if (ModelState.IsValid)
            {
                // save image to wwwroot/Image
                string wwwrootPath = _hostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(blog.ImageFile.FileName);
                string extension = Path.GetExtension(blog.ImageFile.FileName);
                blog.Image = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                string path = Path.Combine(wwwrootPath + "/Image/", fileName);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await blog.ImageFile.CopyToAsync(fileStream);
                }
                string id = HttpContextAccessorProp
                                                .HttpContext
                                                .User
                                                .FindFirst(ClaimTypes.NameIdentifier)
                                                .Value;
                blog.AuthorId = id;

                // Insert to database and save changes
                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        // GET: Blog/Edit/5
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }

        // POST: Blog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, 
            [Bind("BlogId,BlogName,Category,Content,CountOfComments,ImageFile")] Blog blog)
        {
            if (id != blog.BlogId)
            {
                return NotFound();
            }

            var currentBlog = await _context.Blogs.FindAsync(id);

            if (ModelState.IsValid)
            {
                try
                {
                    // if image is also changed then delete old image from folder and database, then add new image
                    if (blog.ImageFile != null)
                    {
                        var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "Image", currentBlog.Image);
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }

                        string wwwrootPath = _hostEnvironment.WebRootPath;
                        string fileName = Path.GetFileNameWithoutExtension(blog.ImageFile.FileName);
                        string extension = Path.GetExtension(blog.ImageFile.FileName);
                        blog.Image = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwrootPath + "/Image/", fileName);
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await blog.ImageFile.CopyToAsync(fileStream);
                        }
                    }
                    // otherwise don't change the old image
                    else
                    {
                        blog.Image = currentBlog.Image; 
                    }

                    blog.CountOfComments = currentBlog.CountOfComments;
                    blog.AuthorId = currentBlog.AuthorId;

                    _context.ChangeTracker.Clear();
                    _context.Update(blog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.BlogId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        [Authorize (Roles ="Admin")]
        // GET: Blog/Delete/5
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(m => m.BlogId == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);

            // delete image from wwwroot/Image
            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "Image", blog.Image);
            if(System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            // delete from database
            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.BlogId == id);
        }
    }
}
