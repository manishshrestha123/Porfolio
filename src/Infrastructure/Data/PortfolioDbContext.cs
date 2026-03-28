using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class PortfolioDbContext : DbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options)
        : base(options)
    {
    }

    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<ContactMessage>();

        e.ToTable("ContactMessages");
        e.HasKey(x => x.Id);
        e.Property(x => x.VisitorName).HasMaxLength(200).IsRequired();
        e.Property(x => x.VisitorEmail).HasMaxLength(320).IsRequired();
        e.Property(x => x.Subject).HasMaxLength(500);
        e.Property(x => x.MessageBody).HasColumnType("text").IsRequired();
        e.Property(x => x.SubmitterIpAddress).HasMaxLength(45);
        e.Property(x => x.AdminNotificationError).HasMaxLength(2000);
        e.Property(x => x.AdminNotificationStatus).HasConversion<byte>();
        e.HasIndex(x => x.CreatedAtUtc);
    }
}
