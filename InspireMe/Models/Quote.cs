using InspireMe.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InspireMe.Models
{
    public class Quote
    {
        [Key]
        public int id { get; set; }
        public string author { get; set; }
        public string content { get; set; }

        public string? ImageUrl { get; set; } 

        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public AppUser User { get; set; }
      
       
    

    }
}
