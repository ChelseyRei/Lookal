using Lookal.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lookal.Web.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Npgsql rejects UTC values for "timestamp without time zone".
        // Storing every DateTime as timestamptz keeps timestamps in UTC.
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp with time zone");
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            entity.HasIndex(u => u.PhoneNumber).IsUnique();
        });
    }
}