
using Microsoft.EntityFrameworkCore;
using BBtaChatbotJackApi.Models; // Add this using statement

namespace BBtaChatbotJackApi.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<PdfDocument> PdfDocuments { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships or other model settings here
            // For example, if you want to define foreign key relationships
            // modelBuilder.Entity<Message>()
            //     .HasOne(m => m.ChatSession)
            //     .WithMany(cs => cs.Messages)
            //     .HasForeignKey(m => m.ChatSessionId);
        }
    }
}