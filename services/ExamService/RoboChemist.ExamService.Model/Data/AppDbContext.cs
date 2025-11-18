using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RoboChemist.ExamService.Model.Models;

namespace RoboChemist.ExamService.Model.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Examquestion> Examquestions { get; set; }

    public virtual DbSet<Examrequest> Examrequests { get; set; }

    public virtual DbSet<Generatedexam> Generatedexams { get; set; }

    public virtual DbSet<Matrix> Matrices { get; set; }

    public virtual DbSet<Matrixdetail> Matrixdetails { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=ep-snowy-cell-a19ensgj-pooler.ap-southeast-1.aws.neon.tech;Port=5432;Database=robochemist_examservice;Username=neondb_owner;Password=npg_CeHyrLVF3pb6;SSL Mode=Require");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Examquestion>(entity =>
        {
            entity.HasKey(e => e.ExamQuestionId).HasName("examquestion_pkey");

            entity.ToTable("examquestion");

            entity.HasIndex(e => new { e.GeneratedExamId, e.QuestionId }, "examquestion_generated_exam_id_question_id_key").IsUnique();

            entity.Property(e => e.ExamQuestionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("exam_question_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.GeneratedExamId).HasColumnName("generated_exam_id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.GeneratedExam).WithMany(p => p.Examquestions)
                .HasForeignKey(d => d.GeneratedExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("examquestion_generated_exam_id_fkey");

            entity.HasOne(d => d.Question).WithMany(p => p.Examquestions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("examquestion_question_id_fkey");
        });

        modelBuilder.Entity<Examrequest>(entity =>
        {
            entity.HasKey(e => e.ExamRequestId).HasName("examrequest_pkey");

            entity.ToTable("examrequest");

            entity.Property(e => e.ExamRequestId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("exam_request_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.MatrixId).HasColumnName("matrix_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Matrix).WithMany(p => p.Examrequests)
                .HasForeignKey(d => d.MatrixId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("examrequest_matrix_id_fkey");
        });

        modelBuilder.Entity<Generatedexam>(entity =>
        {
            entity.HasKey(e => e.GeneratedExamId).HasName("generatedexam_pkey");

            entity.ToTable("generatedexam");

            entity.HasIndex(e => e.ExportedAt, "idx_generatedexam_exported_at");

            entity.HasIndex(e => e.ExportedBy, "idx_generatedexam_exported_by");

            entity.Property(e => e.GeneratedExamId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("generated_exam_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ExamRequestId).HasColumnName("exam_request_id");
            entity.Property(e => e.ExportedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("exported_at");
            entity.Property(e => e.ExportedBy).HasColumnName("exported_by");
            entity.Property(e => e.ExportedFileName)
                .HasMaxLength(255)
                .HasColumnName("exported_file_name");
            entity.Property(e => e.FileFormat)
                .HasMaxLength(10)
                .HasColumnName("file_format");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.ExamRequest).WithMany(p => p.Generatedexams)
                .HasForeignKey(d => d.ExamRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("generatedexam_exam_request_id_fkey");
        });

        modelBuilder.Entity<Matrix>(entity =>
        {
            entity.HasKey(e => e.MatrixId).HasName("matrix_pkey");

            entity.ToTable("matrix");

            entity.Property(e => e.MatrixId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("matrix_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.TotalQuestion).HasColumnName("total_question");
        });

        modelBuilder.Entity<Matrixdetail>(entity =>
        {
            entity.HasKey(e => e.MatrixDetailsId).HasName("matrixdetails_pkey");

            entity.ToTable("matrixdetails");

            entity.HasIndex(e => e.Level, "idx_matrixdetails_level");

            entity.Property(e => e.MatrixDetailsId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("matrix_details_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Level)
                .HasMaxLength(50)
                .HasColumnName("level");
            entity.Property(e => e.MatrixId).HasColumnName("matrix_id");
            entity.Property(e => e.QuestionCount).HasColumnName("question_count");
            entity.Property(e => e.QuestionType)
                .HasMaxLength(50)
                .HasColumnName("question_type");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");

            entity.HasOne(d => d.Matrix).WithMany(p => p.Matrixdetails)
                .HasForeignKey(d => d.MatrixId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("matrixdetails_matrix_id_fkey");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("options_pkey");

            entity.ToTable("options");

            entity.Property(e => e.OptionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("option_id");
            entity.Property(e => e.Answer).HasColumnName("answer");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(false)
                .HasColumnName("is_correct");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");

            entity.HasOne(d => d.Question).WithMany(p => p.Options)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("options_question_id_fkey");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("questions_pkey");

            entity.ToTable("questions");

            entity.HasIndex(e => e.Level, "idx_questions_level");

            entity.HasIndex(e => new { e.TopicId, e.QuestionType, e.Level }, "idx_questions_topic_type_level");

            entity.Property(e => e.QuestionId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("question_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Explanation).HasColumnName("explanation");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Level)
                .HasMaxLength(50)
                .HasColumnName("level");
            entity.Property(e => e.QuestionText).HasColumnName("question_text");
            entity.Property(e => e.QuestionType)
                .HasMaxLength(50)
                .HasColumnName("question_type");
            entity.Property(e => e.TopicId).HasColumnName("topic_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
