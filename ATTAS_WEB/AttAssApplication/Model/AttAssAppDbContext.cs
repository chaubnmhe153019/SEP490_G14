using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AttAssApplication.Model;

namespace AttAssApplication.Model;

public partial class AttAssAppDbContext : DbContext
{
    public AttAssAppDbContext()
    {
    }

    public AttAssAppDbContext(DbContextOptions<AttAssAppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<Solution> Solutions { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<Time> Times { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("PRN231DB"));

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.ToTable("instructor");

            entity.Property(e => e.InstructorId).HasColumnName("instructorId");
            entity.Property(e => e.BusinessId)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("businessId");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.SessionId).HasColumnName("sessionId");

            entity.HasOne(d => d.Session).WithMany(p => p.Instructors)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_instructor_session");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.ToTable("result");

            entity.Property(e => e.ResultId).HasColumnName("resultId");
            entity.Property(e => e.InstructorId).HasColumnName("instructorId");
            entity.Property(e => e.SolutionId).HasColumnName("solutionId");
            entity.Property(e => e.TaskId).HasColumnName("taskId");
            entity.Property(e => e.TimeId).HasColumnName("timeId");

            entity.HasOne(d => d.Instructor).WithMany(p => p.Results)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_result_instructor");

            entity.HasOne(d => d.Solution).WithMany(p => p.Results)
                .HasForeignKey(d => d.SolutionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_result_solution");

            entity.HasOne(d => d.Task).WithMany(p => p.Results)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_result_task");

            entity.HasOne(d => d.Time).WithMany(p => p.Results)
                .HasForeignKey(d => d.TimeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_result_time");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("session");

            entity.Property(e => e.SessionId).HasColumnName("sessionId");
            entity.Property(e => e.SessionHash)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("sessionHash");
            entity.Property(e => e.SolutionCount).HasColumnName("solutionCount");
            entity.Property(e => e.StatusId).HasColumnName("statusId");

            entity.HasOne(d => d.Status).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_session_status");
        });

        modelBuilder.Entity<Solution>(entity =>
        {
            entity.ToTable("solution");

            entity.Property(e => e.SolutionId).HasColumnName("solutionId");
            entity.Property(e => e.No).HasColumnName("no");
            entity.Property(e => e.QuotaAvalable).HasColumnName("quotaAvalable");
            entity.Property(e => e.SessionId).HasColumnName("sessionId");
            entity.Property(e => e.SlotCompability).HasColumnName("slotCompability");
            entity.Property(e => e.SlotPreference).HasColumnName("slotPreference");
            entity.Property(e => e.SubjectDiversity).HasColumnName("subjectDiversity");
            entity.Property(e => e.SubjectPreference).HasColumnName("subjectPreference");
            entity.Property(e => e.TaskAssigned).HasColumnName("taskAssigned");
            entity.Property(e => e.WalkingDistance).HasColumnName("walkingDistance");

            entity.HasOne(d => d.Session).WithMany(p => p.Solutions)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_solution_session");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.ToTable("status");

            entity.Property(e => e.StatusId).HasColumnName("statusId");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.ToTable("task");

            entity.Property(e => e.TaskId).HasColumnName("taskId");
            entity.Property(e => e.BusinessId)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("businessId");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.SessionId).HasColumnName("sessionId");

            entity.HasOne(d => d.Session).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_task_session");
        });

        modelBuilder.Entity<Time>(entity =>
        {
            entity.ToTable("time");

            entity.Property(e => e.TimeId).HasColumnName("timeId");
            entity.Property(e => e.BusinessId)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("businessId");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.SessionId).HasColumnName("sessionId");

            entity.HasOne(d => d.Session).WithMany(p => p.Times)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_time_session");
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.ToTable("token");

            entity.Property(e => e.TokenId).HasColumnName("tokenId");
            entity.Property(e => e.TokenHash)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tokenHash");
            entity.Property(e => e.User)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public DbSet<AttAssApplication.Model.User>? User { get; set; }
}
