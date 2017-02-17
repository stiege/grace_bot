using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GraceBot.Models
{
    public class TwitterQuestion
    {
        [Key]
        [Required]
        public string Id { get; private set; }
        [Required]
        public string Text { get; set; }
        [Required]
        public string StatusId { get; set; }
        [Required]
        public string UserScreenName { get; set; }

        [Required]
        public DateTime Timestamp { get; private set; }

        public TwitterQuestion()
        {
            Id = Guid.NewGuid().ToString();
            Timestamp=DateTime.UtcNow;
        }
    }
}