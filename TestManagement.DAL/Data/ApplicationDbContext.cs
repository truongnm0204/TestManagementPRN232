using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
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
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<StudentClass> StudentClasses { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<ExamAssignment> ExamAssignments { get; set; }
    public DbSet<ExamAttempt> ExamAttempts { get; set; }
    public DbSet<StudentAnswer> StudentAnswers { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<UserLoginLog> UserLoginLogs { get; set; }
    public DbSet<QuestionAuditLog> QuestionAuditLogs { get; set; }
    public DbSet<ExamAuditLog> ExamAuditLogs { get; set; }
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

        builder.Entity<Question>(entity =>
        {
            entity.HasOne(e => e.Topic)
                .WithMany(t => t.Questions)
                .HasForeignKey(e => e.TopicId)
                .OnDelete(DeleteBehavior.SetNull);
        });
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

        builder.Entity<Topic>(entity =>
        {
            entity.HasIndex(e => new { e.SubjectId, e.Name }).IsUnique();
            entity.HasOne(e => e.Subject)
            .WithMany()
            .HasForeignKey(e => e.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
        });
        // === Class ===
        builder.Entity<Class>(entity =>
        {
            entity.HasIndex(e => e.ClassCode).IsUnique();
        });

        // === StudentClass ===
        builder.Entity<StudentClass>(entity =>
        {
            entity.HasIndex(e => new { e.StudentId, e.ClassId }).IsUnique();
            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Class)
                .WithMany(c => c.StudentClasses)
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // === Exam ===
        builder.Entity<Exam>(entity =>
        {
            entity.HasIndex(e => new { e.SubjectId, e.Status, e.IsDeleted });
            entity.HasOne(e => e.Subject)
                .WithMany()
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // === ExamAssignment ===
        builder.Entity<ExamAssignment>(entity =>
        {
            entity.HasIndex(e => new { e.ExamId, e.ClassId }).IsUnique();
            entity.HasOne(e => e.Exam)
                .WithMany(ex => ex.ExamAssignments)
                .HasForeignKey(e => e.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Class)
                .WithMany(c => c.ExamAssignments)
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Assigner)
                .WithMany()
                .HasForeignKey(e => e.AssignedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // === ExamAttempt ===
        builder.Entity<ExamAttempt>(entity =>
        {
            entity.HasIndex(e => new { e.ExamId, e.StudentId });
            entity.HasOne(e => e.Exam)
                .WithMany(ex => ex.ExamAttempts)
                .HasForeignKey(e => e.ExamId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // === StudentAnswer ===
        builder.Entity<StudentAnswer>(entity =>
        {
            entity.HasIndex(e => new { e.ExamAttemptId, e.QuestionId }).IsUnique();
            entity.HasOne(e => e.ExamAttempt)
                .WithMany(a => a.StudentAnswers)
                .HasForeignKey(e => e.ExamAttemptId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Question)
                .WithMany()
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.SelectedOption)
                .WithMany()
                .HasForeignKey(e => e.SelectedOptionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // === Notification ===
        builder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.IsRead });
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // === UserLoginLog ===
        builder.Entity<UserLoginLog>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // === QuestionAuditLog ===
        builder.Entity<QuestionAuditLog>(entity =>
        {
            entity.HasIndex(e => e.QuestionId);
            entity.HasOne(e => e.Question)
                .WithMany()
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Actor)
                .WithMany()
                .HasForeignKey(e => e.ActionBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // === ExamAuditLog ===
        builder.Entity<ExamAuditLog>(entity =>
        {
            entity.HasIndex(e => e.ExamId);
            entity.HasOne(e => e.Exam)
                .WithMany()
                .HasForeignKey(e => e.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Actor)
                .WithMany()
                .HasForeignKey(e => e.ActionBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

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
