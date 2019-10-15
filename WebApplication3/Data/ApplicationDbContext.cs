using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Models;

namespace WebApplication3.Data
{
    public class ApplicationDbContext : IdentityDbContext<User,IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            //Database.EnsureCreated();
        }
        public new DbSet<User> Users { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<SingleChoiceQuestion> SingleChoiceQuestions { get; set; }
        public DbSet<MultiChoiceQuestion> MultiChoiceQuestions { get; set; }
        public DbSet<TextQuestion> TextQuestions { get; set; }
        public DbSet<DragAndDropQuestion> DragAndDropQuestions { get; set; }
        public DbSet<CodeQuestion> CodeQuestions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<SingleChoiceAnswer> SingleChoiceAnswers { get; set; }
        public DbSet<MultiChoiceAnswer> MultiChoiceAnswers { get; set; }
        public DbSet<TextAnswer> TextAnswers { get; set; }
        public DbSet<DragAndDropAnswer> DragAndDropAnswers { get; set; }
        public DbSet<CodeAnswer> CodeAnswers { get; set; }
        public DbSet<DragAndDropAnswerOption> DragAndDropAnswerOptions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<Code> Codes { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Question>(q =>
            {
                q.HasDiscriminator<string>("QuestionType");
                q.ToTable("Question");
                q.Property(e => e.QuestionType)
                    .HasMaxLength(50).HasColumnName("question_type");
                q.Property(p => p.Score).HasDefaultValue(1);
                q.Property(p => p.IsDeleted)
                    .HasDefaultValue(false);
                q.HasOne(c => c.Test)
                    .WithMany(c => c.Questions)
                    .HasForeignKey(c => c.TestId).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<MultiChoiceQuestion>().ToTable("MultiChoiceQuestions");
            builder.Entity<SingleChoiceQuestion>().ToTable("SingleChoiceQuestions");
            builder.Entity<TextQuestion>().ToTable("TextQuestions");
            builder.Entity<DragAndDropQuestion>().ToTable("DragAndDropQuestions");
            builder.Entity<CodeQuestion>().ToTable("CodeQuestions");

            builder.Entity<Answer>(a =>
            {
                a.HasDiscriminator<string>("AnswerType");
                a.ToTable("Answers");
                a.Property(e => e.AnswerType)
                    .HasMaxLength(50).HasColumnName("answer_type");
                a.HasOne(c => c.Question)
                    .WithMany()
                    .HasForeignKey(c => c.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<MultiChoiceAnswer>().ToTable("MultiChoiceAnswers");
            builder.Entity<SingleChoiceAnswer>().ToTable("SingleChoiceAnswers");
            builder.Entity<TextAnswer>().ToTable("TextAnswers");
            builder.Entity<DragAndDropAnswer>().ToTable("DragAndDropAnswers");
            builder.Entity<CodeAnswer>().ToTable("CodeAnswers");
            builder.Entity<DragAndDropAnswerOption>().ToTable("DragAndDropAnswerOptions")
                .HasOne(c=>c.Option)
                .WithMany(c=>c.DropAnswerOptions)
                .HasForeignKey(c=>c.OptionId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<DragAndDropAnswerOption>().ToTable("DragAndDropAnswerOptions")
                .HasOne(c=>c.RightOption)
                .WithMany()
                .HasForeignKey(c=>c.RightOptionId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Option>().ToTable("Options");
            builder.Entity<Test>(e =>
            {
                e.ToTable("Tests");
                e.Property("IsDeleted")
                    .HasDefaultValue(false);
            });
            builder.Entity<TestResult>().ToTable("TestResults")
                .HasOne(c=>c.Test)
                .WithMany(t=>t.TestResults)
                .HasForeignKey(t=>t.TestId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<User>().ToTable("Users");
            builder.Entity<AnswerOption>().ToTable("AnswerOptions");
            builder.Entity<Code>().ToTable("Codes");
            builder.Entity<Code>().HasOne(c => c.Answer).WithMany().HasForeignKey("AnswerId");
            builder.Entity<Code>().HasOne(c => c.Test).WithMany().HasForeignKey("TestId");
            builder.Entity<Code>().HasOne(c => c.Question).WithMany().HasForeignKey("QuestionId");
            // many-to-many
            builder.Entity<AnswerOption>()
                .HasKey(ao => new { ao.AnswerId, ao.OptionId });
            builder.Entity<AnswerOption>()
                .HasOne(ao => ao.Answer)
                .WithMany(a => a.AnswerOptions)
                .HasForeignKey(ao => ao.AnswerId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<AnswerOption>()
                .HasOne(ao => ao.Option)
                .WithMany(o => o.AnswerOptions)
                .HasForeignKey(ao => ao.OptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
