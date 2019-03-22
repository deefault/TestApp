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
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<SingleChoiceQuestion> SingleChoiceQuestions { get; set; }
        public DbSet<MultiChoiceQuestion> MultiChoiceQuestions { get; set; }
        public DbSet<TextQuestion> TextQuestions { get; set; }
        public DbSet<DragAndDropQuestion> DragAndDropQuestions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<SingleChoiceAnswer> SingleChoiceAnswers { get; set; }
        public DbSet<MultiChoiceAnswer> MultiChoiceAnswers { get; set; }
        public DbSet<TextAnswer> TextAnswers { get; set; }
        public DbSet<DragAndDropAnswer> DragAndDropAnswers { get; set; }
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
            });
            builder.Entity<MultiChoiceQuestion>().ToTable("MultiChoiceQuestion");
            builder.Entity<SingleChoiceQuestion>().ToTable("SingleChoiceQuestion");
            builder.Entity<TextQuestion>().ToTable("TextQuestion");
            builder.Entity<DragAndDropQuestion>().ToTable("DragAndDropQuestion");
            
            builder.Entity<Answer>(a =>
            {
                a.HasDiscriminator<string>("AnswerType");
                a.ToTable("Answer");
                a.Property(e => e.AnswerType)
                    .HasMaxLength(50).HasColumnName("answer_type");
            });
            builder.Entity<MultiChoiceAnswer>().ToTable("MultiChoiceAnswer");
            builder.Entity<SingleChoiceAnswer>().ToTable("SingleChoiceAnswer");
            builder.Entity<TextAnswer>().ToTable("TextAnswer");
            builder.Entity<DragAndDropAnswer>().ToTable("DragAndDropAnswer");
            builder.Entity<DragAndDropAnswerOption>().ToTable("DragAndDropAnswerOption");
            
            builder.Entity<Option>().ToTable("Option");
            builder.Entity<Test>().ToTable("Test");
            builder.Entity<TestResult>().ToTable("TestResult");
            builder.Entity<User>().ToTable("User");
            builder.Entity<AnswerOption>().ToTable("AnswerOptions");
            builder.Entity<Code>().ToTable("Codes");
            
            // many-to-many
            builder.Entity<AnswerOption>()
                .HasKey(ao => new { ao.AnswerId, ao.OptionId });
            builder.Entity<AnswerOption>()
                .HasOne(ao => ao.Answer)
                .WithMany(a => a.AnswerOptions)
                .HasForeignKey(ao => ao.AnswerId);
            builder.Entity<AnswerOption>()
                .HasOne(ao => ao.Option)
                .WithMany(o => o.AnswerOptions)
                .HasForeignKey(ao => ao.OptionId);
        }

    }
}
