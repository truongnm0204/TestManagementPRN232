using Microsoft.EntityFrameworkCore;
using TestManagement.DAL.Models;

namespace TestManagement.DAL.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        builder.Entity<User>()
            .HasIndex(x => new { x.Role, x.IsActive, x.IsDeleted });

        builder.Entity<Subject>()
            .HasIndex(x => x.Code)
            .IsUnique();

        builder.Entity<Subject>()
            .HasIndex(x => new { x.Status, x.IsDeleted });

        builder.Entity<Question>()
            .HasIndex(x => new { x.SubjectId, x.Difficulty, x.Status, x.IsDeleted });

        builder.Entity<QuestionOption>()
            .HasIndex(x => new { x.QuestionId, x.Label })
            .IsUnique();

        builder.Entity<Subject>()
            .HasMany(x => x.Questions)
            .WithOne(x => x.Subject)
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Question>()
            .HasMany(x => x.Options)
            .WithOne(x => x.Question)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        SeedData(builder);
    }

    private static void SeedData(ModelBuilder builder)
    {
        builder.Entity<User>().HasData(new User
        {
            Id = 1,
            FullName = "System Admin",
            Email = "admin@testmanagement.com",
            PasswordHash = "$2a$11$Z3n2vbwVmWHMlkswAWCJVe8FUWE9iFQXLOlrfOafHrfjcN2qkFF5G",
            PhoneNumber = "0900000000",
            Role = "Admin",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = new DateTime(2026, 1, 1)
        });

        builder.Entity<Subject>().HasData(
            new Subject
            {
                Id = 1,
                Code = "ENG101",
                Name = "English Foundation",
                Description = "Basic English grammar and vocabulary.",
                Status = "Active",
                CreatedAt = new DateTime(2026, 1, 1)
            },
            new Subject
            {
                Id = 2,
                Code = "IELTS101",
                Name = "IELTS Preparation",
                Description = "IELTS reading and listening practice.",
                Status = "Active",
                CreatedAt = new DateTime(2026, 1, 1)
            }
        );
    }
}
