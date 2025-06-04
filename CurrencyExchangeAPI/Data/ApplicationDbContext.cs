using Microsoft.EntityFrameworkCore;
using CurrencyExchangeAPI.Models;

namespace CurrencyExchangeAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ExchangeRate> ExchangeRates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExchangeRate>()
                .HasIndex(e => new { e.BaseCurrency, e.TargetCurrency, e.Timestamp });
        }
    }
} 