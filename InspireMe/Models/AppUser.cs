using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace InspireMe.Models
{
    public class AppUser:IdentityUser
    {
        public string Name { get; set; }
        public List<Quote> Quotes { get; set; }
    }
 }
