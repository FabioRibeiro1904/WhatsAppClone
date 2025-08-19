using Microsoft.EntityFrameworkCore;
using WhatsAppClone.Models;

namespace WhatsAppClone.Data;

public class WhatsAppDbContext : DbContext
{
    public WhatsAppDbContext(DbContextOptions<WhatsAppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<ChatParticipant> ChatParticipants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.Property(e => e.Status)
                .HasDefaultValue("Hey there! I am using WhatsApp Clone.");
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasOne(d => d.CreatedBy)
                .WithMany()
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasOne(d => d.Sender)
                .WithMany(p => p.SentMessages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Chat)
                .WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.ReplyToMessage)
                .WithMany(p => p.Replies)
                .HasForeignKey(d => d.ReplyToMessageId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.Type)
                .HasConversion<string>();
                
            entity.Property(e => e.Status)
                .HasConversion<string>();
        });

        modelBuilder.Entity<ChatParticipant>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ChatId });

            entity.HasOne(d => d.User)
                .WithMany(p => p.ChatParticipants)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Chat)
                .WithMany(p => p.Participants)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Role)
                .HasConversion<string>();
        });
    }
}
