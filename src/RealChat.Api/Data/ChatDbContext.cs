using Microsoft.EntityFrameworkCore;
using RealChat.Api.Models;

namespace RealChat.Api.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(256);
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Message>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Data).IsRequired().HasMaxLength(MessageValidator.MaxDataLength);
            e.HasOne(x => x.Sender).WithMany(x => x.MessagesSent).HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Receiver).WithMany(x => x.MessagesReceived).HasForeignKey(x => x.ReceiverId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.SenderId, x.ReceiverId, x.SentAt });
        });
    }
}
