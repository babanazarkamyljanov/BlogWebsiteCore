using BlogWebsiteCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogWebsiteCore.ViewModels
{
    public class BlogCommentViewModel
    {
        public Blog Blog { get; set; }
        public Comment Comment { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
