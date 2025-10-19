using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RoboChemist.SlidesService.Model.Models;

namespace RoboChemist.SlidesService.Model.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Generatedslide> Generatedslides { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<Sliderequest> Sliderequests { get; set; }

    public virtual DbSet<Syllabus> Syllabi { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Generatedslide>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("generatedslide_pkey");

            entity.ToTable("generatedslide");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.FileFormat)
                .HasMaxLength(50)
                .HasColumnName("file_format");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FilePath)
                .HasMaxLength(500)
                .HasColumnName("file_path");
            entity.Property(e => e.FileSize).HasColumnName("file_size");
            entity.Property(e => e.GeneratedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("generated_at");
            entity.Property(e => e.GenerationStatus)
                .HasMaxLength(50)
                .HasColumnName("generation_status");
            entity.Property(e => e.JsonContent).HasColumnName("json_content");
            entity.Property(e => e.Metadata).HasColumnName("metadata");
            entity.Property(e => e.ProcessingTime).HasColumnName("processing_time");
            entity.Property(e => e.SlideCount).HasColumnName("slide_count");
            entity.Property(e => e.SlideRequestId).HasColumnName("slide_request_id");

            entity.HasOne(d => d.SlideRequest).WithMany(p => p.Generatedslides)
                .HasForeignKey(d => d.SlideRequestId)
                .HasConstraintName("generatedslide_slide_request_id_fkey");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("grade_pkey");

            entity.ToTable("grade");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.GradeName)
                .HasMaxLength(255)
                .HasColumnName("grade_name");
        });

        modelBuilder.Entity<Sliderequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sliderequest_pkey");

            entity.ToTable("sliderequest");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AiPrompt).HasColumnName("ai_prompt");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_at");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.NumberOfSlides).HasColumnName("number_of_slides");
            entity.Property(e => e.RequestType)
                .HasMaxLength(50)
                .HasColumnName("request_type");
            entity.Property(e => e.RequestedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("requested_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.SyllabusId).HasColumnName("syllabus_id");
            entity.Property(e => e.TemplateStyle)
                .HasMaxLength(255)
                .HasColumnName("template_style");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserRequirements).HasColumnName("user_requirements");

            entity.HasOne(d => d.Syllabus).WithMany(p => p.Sliderequests)
                .HasForeignKey(d => d.SyllabusId)
                .HasConstraintName("sliderequest_syllabus_id_fkey");
        });

        modelBuilder.Entity<Syllabus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("syllabus_pkey");

            entity.ToTable("syllabus");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ContentOutline).HasColumnName("content_outline");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.KeyConcepts).HasColumnName("key_concepts");
            entity.Property(e => e.LearningObjectives).HasColumnName("learning_objectives");
            entity.Property(e => e.Lesson)
                .HasMaxLength(255)
                .HasColumnName("lesson");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(255)
                .HasColumnName("subject");
            entity.Property(e => e.TeachingNotes).HasColumnName("teaching_notes");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Topic).WithMany(p => p.Syllabi)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("syllabus_topic_id_fkey");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("topic_pkey");

            entity.ToTable("topic");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.GradeId).HasColumnName("grade_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.TopicName)
                .HasMaxLength(255)
                .HasColumnName("topic_name");

            entity.HasOne(d => d.Grade).WithMany(p => p.Topics)
                .HasForeignKey(d => d.GradeId)
                .HasConstraintName("topic_grade_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
