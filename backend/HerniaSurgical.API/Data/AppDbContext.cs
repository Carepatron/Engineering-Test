using Microsoft.EntityFrameworkCore;
using HerniaSurgical.API.Models;

namespace HerniaSurgical.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Conversation -> Messages relationship
            modelBuilder.Entity<Conversation>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Client -> Conversations relationship (optional)
            modelBuilder.Entity<Client>()
                .HasMany(c => c.Conversations)
                .WithOne(c => c.Client)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Client -> Appointments relationship
            modelBuilder.Entity<Client>()
                .HasMany(c => c.Appointments)
                .WithOne(a => a.Client)
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Conversations relationship (optional)
            modelBuilder.Entity<User>()
                .HasMany(u => u.CreatedConversations)
                .WithOne(c => c.CreatedByUser)
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // User -> Messages relationship (optional)
            modelBuilder.Entity<User>()
                .HasMany(u => u.SentMessages)
                .WithOne(m => m.SenderUser)
                .HasForeignKey(m => m.SenderUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Indexes for performance
            modelBuilder.Entity<Message>()
                .HasIndex(m => m.ConversationId);
            
            modelBuilder.Entity<Conversation>()
                .HasIndex(c => c.ClientId);
            
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.ClientId);
            
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.AppointmentDate);
            
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // User indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
            modelBuilder.Entity<Message>()
                .HasIndex(m => m.SenderUserId);
            
            modelBuilder.Entity<Conversation>()
                .HasIndex(c => c.CreatedByUserId);
        }
    }
}