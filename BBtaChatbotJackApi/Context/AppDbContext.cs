
using Microsoft.EntityFrameworkCore;
using BBtaChatbotJackApi.Models; // Add this using statement

namespace BBtaChatbotJackApi.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<PdfDocuments> PdfDocuments { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<FileInfos> FileInfos { get; internal set; }
        
        public DbSet<FileInfo> FileInform { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .HasOne(m => m.ChatSession)
                .WithMany(cs => cs.Messages)
                .HasForeignKey(m => m.ChatSessionId);
        }
    }
}