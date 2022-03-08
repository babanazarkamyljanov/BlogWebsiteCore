using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlogWebsiteCore.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        [Required]
        public string Message { get; set; }
        public string AuthorId { get; set; }
        public int BlogId { get; set; }
        public DateTime CreatedDate { get; set; }

        public Comment()
        {
            CreatedDate = DateTime.Now;
        }
    }
}
