using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace BlogWebsiteCore.Models
{
    public class Blog
    {
        public int BlogId { get; set; }

        [Required]
        [Display(Name ="Blog Title")]
        public string BlogName { get; set; }

        [Required]
        [Display(Name ="Blog Category")]
        public string Category { get; set; }

        [Required]
        [Display(Name ="Blog Content")]
        public string Content { get; set; }

        public DateTime CreatedDate { get; set; }

        public string AuthorId { get; set; }

        public string Image { get; set; }

        [NotMapped]
        [Display(Name ="Upload File")]
        public IFormFile ImageFile { get; set; }

        public int CountOfComments { get; set; }

        public Blog()
        {
            CreatedDate = DateTime.Now;
        }
    }
}
