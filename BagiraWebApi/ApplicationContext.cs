using BagiraWebApi.Models.Bagira;
using BagiraWebApi.Models.Parser;
using Microsoft.EntityFrameworkCore;

namespace BagiraWebApi
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Good> Goods { get; set; } = null!;
        public DbSet<GoodStorage> GoodStorages { get; set; } = null!;
        public DbSet<GoodRest> GoodRests { get; set; } = null!;
        public DbSet<GoodPriceType> GoodPriceTypes { get; set; } = null!;
        public DbSet<GoodPrice> GoodPrices { get; set; } = null!;
        public DbSet<GoodPropertyValue> GoodPropertyValues { get; set; } = null!;

        //Parser
        public DbSet<ParserGood> ParserGoods { get; set; } = null!;
        public DbSet<ParserPage> ParserPages { get; set; } = null!;

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Good>().Property(g => g.Id).ValueGeneratedNever();
            modelBuilder.Entity<Good>().HasIndex(g => g.Path);
            modelBuilder.Entity<GoodStorage>().Property(g => g.Id).ValueGeneratedNever();
            modelBuilder.Entity<GoodRest>().HasKey(gr => new { gr.GoodId, gr.StorageId });
            modelBuilder.Entity<GoodPriceType>().Property(g => g.Id).ValueGeneratedNever();
            modelBuilder.Entity<GoodPrice>().HasKey(p => new { p.GoodId, p.PriceTypeId });
            modelBuilder.Entity<GoodPrice>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<GoodPropertyValue>().HasKey(gp => new { gp.GoodId, gp.PropertyId });

            modelBuilder.Entity<ParserGood>().HasKey(pg => new { pg.Id, pg.ParserCompanyId });
        }
    }
}
