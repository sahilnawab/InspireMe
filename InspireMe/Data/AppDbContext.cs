using InspireMe.Models;
using Microsoft.EntityFrameworkCore;

namespace InspireMe.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Quote> Quotes { get; set; }
    }
}
