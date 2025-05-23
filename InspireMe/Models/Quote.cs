using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InspireMe.Models
{
    public class Quote
    {
        [Key]
        public int id { get; set; }
        public string author { get; set; }
        public string content { get; set; }

    }
}
