﻿using BagiraWebApi.Models.Bagira;
using Microsoft.EntityFrameworkCore;

namespace BagiraWebApi
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Good> Goods { get; set; } = null!;
        public DbSet<GoodStorage> GoodStorages { get; set; } = null!;
        public DbSet<GoodPriceType> GoodPriceTypes { get; set; } = null!;

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Good>().Property(g => g.Id).ValueGeneratedNever();
            modelBuilder.Entity<GoodStorage>().Property(g => g.Id).ValueGeneratedNever();
            modelBuilder.Entity<GoodPriceType>().Property(g => g.Id).ValueGeneratedNever();
        }
    }
}
