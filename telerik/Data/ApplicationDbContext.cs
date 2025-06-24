using Microsoft.EntityFrameworkCore;
using telerik.Models;

namespace telerik.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }  // <-- Add this line
    }
}
