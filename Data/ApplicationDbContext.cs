// No changes from your original code needed for ApplicationDbContext.cs related to the new GameSession properties
// EF Core will map QuestionOrderJson as a string and CurrentQuestionIndex as an int automatically.
// The existing seed data and model configurations are fine.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizAppBackend.Models;

namespace QuizAppBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<QuizCategory> QuizCategories { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<GameSessionPlayer> GameSessionPlayers { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<ScoreEntry> ScoreEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Friendship: Prevent cascade delete for both Sender and Receiver
            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Sender)
                .WithMany(u => u.FriendshipsSent)
                .HasForeignKey(f => f.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Receiver)
                .WithMany(u => u.FriendshipsReceived)
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasIndex(f => new { f.SenderId, f.ReceiverId })
                .IsUnique();

            // GameSessionPlayer: Cascade when GameSession is deleted
            modelBuilder.Entity<GameSessionPlayer>()
                .HasOne(gsp => gsp.GameSession)
                .WithMany(gs => gs.Players)
                .HasForeignKey(gsp => gsp.GameSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameSessionPlayer>()
                .HasOne(gsp => gsp.User)
                .WithMany(u => u.GameSessionsPlayed)
                .HasForeignKey(gsp => gsp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserAnswer FKs – all with Restrict
            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany()
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); // good, Restrict is fine

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.GameSession)
                .WithMany()
                .HasForeignKey(ua => ua.GameSessionId)
                .OnDelete(DeleteBehavior.Restrict); // good

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Restrict); // or .NoAction()

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.GameSessionPlayer)
                .WithMany()
                .HasForeignKey(ua => ua.GameSessionPlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ScoreEntry FKs – Restrict
            modelBuilder.Entity<ScoreEntry>()
                .HasOne(se => se.User)
                .WithMany(u => u.ScoreEntries)
                .HasForeignKey(se => se.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ScoreEntry>()
                .HasOne(se => se.QuizCategory)
                .WithMany()
                .HasForeignKey(se => se.QuizCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // QuizCategory → Question – Cascade is OK
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Category)
                .WithMany(qc => qc.Questions)
                .HasForeignKey(q => q.QuizCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enum conversions
            modelBuilder.Entity<Question>()
                .Property(q => q.Difficulty)
                .HasConversion<string>();

            modelBuilder.Entity<Question>()
                .Property(q => q.Type)
                .HasConversion<string>();

            modelBuilder.Entity<ScoreEntry>()
                .Property(s => s.Difficulty)
                .HasConversion<string>();

            // Seed data
            modelBuilder.Entity<QuizCategory>().HasData(
                new QuizCategory { Id = 1, Name = "Historia", Description = "Frågor om historiska händelser och personer." },
                new QuizCategory { Id = 2, Name = "Vetenskap", Description = "Frågor om naturvetenskap och teknologi." },
                new QuizCategory { Id = 3, Name = "Geografi", Description = "Frågor om länder, städer och fysisk geografi." }
            );

            modelBuilder.Entity<Question>().HasData(
                new Question { Id = 1, QuizCategoryId = 1, Text = "Vem var Sveriges kung under Slaget vid Poltava?", OptionsJson = "[\"Karl X Gustav\", \"Karl XI\", \"Karl XII\", \"Gustav II Adolf\"]", CorrectAnswer = "Karl XII", Difficulty = DifficultyLevel.Medium, Type = QuestionType.MultipleChoice },
                new Question { Id = 2, QuizCategoryId = 1, Text = "Var andra världskrigetade mellan 1939 och 1945?", OptionsJson = "[\"Sant\", \"Falskt\"]", CorrectAnswer = "Sant", Difficulty = DifficultyLevel.Easy, Type = QuestionType.TrueFalse },
                new Question { Id = 3, QuizCategoryId = 2, Text = "Vilken är den minsta planeten i solsystemet?", OptionsJson = "[\"Jorden\", \"Mars\", \"Merkurius\", \"Venus\"]", CorrectAnswer = "Merkurius", Difficulty = DifficultyLevel.Easy, Type = QuestionType.MultipleChoice },
                new Question { Id = 4, QuizCategoryId = 2, Text = "Är vatten ett grundämne?", OptionsJson = "[\"Sant\", \"Falskt\"]", CorrectAnswer = "Falskt", Difficulty = DifficultyLevel.Easy, Type = QuestionType.TrueFalse }
            );
        }
    }
}