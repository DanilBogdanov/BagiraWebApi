using BagiraWebApi.Models.Bagira;
using Microsoft.EntityFrameworkCore;

namespace BagiraWebApi
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Good> Goods { get; set; } = null!;

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Good>().Property(g => g.Id).ValueGeneratedNever();
        }
    }
}
