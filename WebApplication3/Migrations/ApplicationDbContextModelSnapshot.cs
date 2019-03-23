﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebApplication3.Data;

namespace WebApplication3.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.8-servicing-32085");

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<int>("RoleId");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<int>("UserId");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.Property<int>("UserId");

                    b.Property<int>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.Property<int>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("WebApplication3.Models.Answer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AnswerType")
                        .IsRequired()
                        .HasColumnName("answer_type")
                        .HasMaxLength(50);

                    b.Property<ushort>("Order");

                    b.Property<int>("QuestionId");

                    b.Property<float>("Score");

                    b.Property<int>("TestResultId");

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.HasIndex("TestResultId");

                    b.ToTable("Answer");

                    b.HasDiscriminator<string>("AnswerType").HasValue("Answer");
                });

            modelBuilder.Entity("WebApplication3.Models.AnswerOption", b =>
                {
                    b.Property<int>("AnswerId");

                    b.Property<int>("OptionId");

                    b.Property<bool>("Checked");

                    b.HasKey("AnswerId", "OptionId");

                    b.HasIndex("OptionId");

                    b.ToTable("AnswerOptions");
                });

            modelBuilder.Entity("WebApplication3.Models.Code", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Args");

                    b.Property<string>("Output");

                    b.Property<int?>("QuestionId");

                    b.Property<int?>("UserId");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.HasIndex("UserId");

                    b.ToTable("Codes");
                });

            modelBuilder.Entity("WebApplication3.Models.DragAndDropAnswerOption", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AnswerId");

                    b.Property<int>("ChosenOrder");

                    b.Property<int>("OptionId");

                    b.Property<int>("RightOptionId");

                    b.HasKey("Id");

                    b.HasIndex("AnswerId");

                    b.HasIndex("RightOptionId");

                    b.ToTable("DragAndDropAnswerOption");
                });

            modelBuilder.Entity("WebApplication3.Models.Option", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsRight");

                    b.Property<int>("Order");

                    b.Property<int>("QuestionId");

                    b.Property<string>("Text")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.ToTable("Option");
                });

            modelBuilder.Entity("WebApplication3.Models.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("QuestionType")
                        .IsRequired()
                        .HasColumnName("question_type")
                        .HasMaxLength(50);

                    b.Property<int>("Score")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(1);

                    b.Property<int>("TestId");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("TestId");

                    b.ToTable("Question");

                    b.HasDiscriminator<string>("QuestionType").HasValue("Question");
                });

            modelBuilder.Entity("WebApplication3.Models.Test", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CreatedById");

                    b.Property<DateTime>("CreatedOn")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("HideRightAnswers");

                    b.Property<bool>("IsEnabled");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<bool>("Shuffled");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.ToTable("Test");
                });

            modelBuilder.Entity("WebApplication3.Models.TestResult", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CompletedByUserId");

                    b.Property<DateTime>("CompletedOn");

                    b.Property<bool>("IsCompleted");

                    b.Property<uint>("RightAnswersCount");

                    b.Property<DateTime>("StartedOn");

                    b.Property<int>("TestId");

                    b.Property<uint>("TotalQuestions");

                    b.HasKey("Id");

                    b.HasIndex("CompletedByUserId");

                    b.HasIndex("TestId");

                    b.ToTable("TestResult");
                });

            modelBuilder.Entity("WebApplication3.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("User");
                });

            modelBuilder.Entity("WebApplication3.Models.DragAndDropAnswer", b =>
                {
                    b.HasBaseType("WebApplication3.Models.Answer");


                    b.ToTable("DragAndDropAnswer");

                    b.HasDiscriminator().HasValue("DragAndDropAnswer");
                });

            modelBuilder.Entity("WebApplication3.Models.MultiChoiceAnswer", b =>
                {
                    b.HasBaseType("WebApplication3.Models.Answer");


                    b.ToTable("MultiChoiceAnswer");

                    b.HasDiscriminator().HasValue("MultiChoiceAnswer");
                });

            modelBuilder.Entity("WebApplication3.Models.SingleChoiceAnswer", b =>
                {
                    b.HasBaseType("WebApplication3.Models.Answer");

                    b.Property<int?>("OptionId");

                    b.HasIndex("OptionId");

                    b.ToTable("SingleChoiceAnswer");

                    b.HasDiscriminator().HasValue("SingleChoiceAnswer");
                });

            modelBuilder.Entity("WebApplication3.Models.TextAnswer", b =>
                {
                    b.HasBaseType("WebApplication3.Models.Answer");

                    b.Property<string>("Text");

                    b.ToTable("TextAnswer");

                    b.HasDiscriminator().HasValue("TextAnswer");
                });

            modelBuilder.Entity("WebApplication3.Models.DragAndDropQuestion", b =>
                {
                    b.HasBaseType("WebApplication3.Models.Question");


                    b.ToTable("DragAndDropQuestion");

                    b.HasDiscriminator().HasValue("DragAndDropQuestion");
                });

            modelBuilder.Entity("WebApplication3.Models.MultiChoiceQuestion", b =>
                {
                    b.HasBaseType("WebApplication3.Models.Question");


                    b.ToTable("MultiChoiceQuestion");

                    b.HasDiscriminator().HasValue("MultiChoiceQuestion");
                });

            modelBuilder.Entity("WebApplication3.Models.SingleChoiceQuestion", b =>
                {
                    b.HasBaseType("WebApplication3.Models.Question");

                    b.Property<int?>("RightAnswerId");

                    b.HasIndex("RightAnswerId");

                    b.ToTable("SingleChoiceQuestion");

                    b.HasDiscriminator().HasValue("SingleChoiceQuestion");
                });

            modelBuilder.Entity("WebApplication3.Models.TextQuestion", b =>
                {
                    b.HasBaseType("WebApplication3.Models.Question");

                    b.Property<string>("TextRightAnswer")
                        .IsRequired();

                    b.ToTable("TextQuestion");

                    b.HasDiscriminator().HasValue("TextQuestion");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole<int>")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.HasOne("WebApplication3.Models.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.HasOne("WebApplication3.Models.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole<int>")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("WebApplication3.Models.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.HasOne("WebApplication3.Models.User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebApplication3.Models.Answer", b =>
                {
                    b.HasOne("WebApplication3.Models.Question", "Question")
                        .WithMany()
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("WebApplication3.Models.TestResult", "TestResult")
                        .WithMany("Answers")
                        .HasForeignKey("TestResultId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebApplication3.Models.AnswerOption", b =>
                {
                    b.HasOne("WebApplication3.Models.MultiChoiceAnswer", "Answer")
                        .WithMany("AnswerOptions")
                        .HasForeignKey("AnswerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("WebApplication3.Models.Option", "Option")
                        .WithMany("AnswerOptions")
                        .HasForeignKey("OptionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebApplication3.Models.Code", b =>
                {
                    b.HasOne("WebApplication3.Models.Question", "Question")
                        .WithMany()
                        .HasForeignKey("QuestionId");

                    b.HasOne("WebApplication3.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("WebApplication3.Models.DragAndDropAnswerOption", b =>
                {
                    b.HasOne("WebApplication3.Models.DragAndDropAnswer", "Answer")
                        .WithMany("DragAndDropAnswerOptions")
                        .HasForeignKey("AnswerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("WebApplication3.Models.Option", "RightOption")
                        .WithMany()
                        .HasForeignKey("RightOptionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebApplication3.Models.Option", b =>
                {
                    b.HasOne("WebApplication3.Models.Question", "Question")
                        .WithMany("Options")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebApplication3.Models.Question", b =>
                {
                    b.HasOne("WebApplication3.Models.Test", "Test")
                        .WithMany("Questions")
                        .HasForeignKey("TestId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebApplication3.Models.Test", b =>
                {
                    b.HasOne("WebApplication3.Models.User", "CreatedBy")
                        .WithMany("Tests")
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebApplication3.Models.TestResult", b =>
                {
                    b.HasOne("WebApplication3.Models.User", "CompletedByUser")
                        .WithMany("TestResults")
                        .HasForeignKey("CompletedByUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("WebApplication3.Models.Test", "Test")
                        .WithMany()
                        .HasForeignKey("TestId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebApplication3.Models.SingleChoiceAnswer", b =>
                {
                    b.HasOne("WebApplication3.Models.Option", "Option")
                        .WithMany()
                        .HasForeignKey("OptionId");
                });

            modelBuilder.Entity("WebApplication3.Models.SingleChoiceQuestion", b =>
                {
                    b.HasOne("WebApplication3.Models.Option", "RightAnswer")
                        .WithMany()
                        .HasForeignKey("RightAnswerId");
                });
#pragma warning restore 612, 618
        }
    }
}
