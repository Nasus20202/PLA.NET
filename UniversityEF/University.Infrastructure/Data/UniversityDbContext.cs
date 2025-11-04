using Microsoft.EntityFrameworkCore;
using University.Domain.Entities;

namespace University.Infrastructure.Data;

/// <summary>
/// Database context for the university system
/// </summary>
public class UniversityDbContext : DbContext
{
    public UniversityDbContext(DbContextOptions<UniversityDbContext> options)
        : base(options) { }

    // DbSets for all entities
    public DbSet<Student> Students { get; set; }
    public DbSet<MasterStudent> MasterStudents { get; set; }
    public DbSet<Professor> Professors { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Department> Faculties { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Office> Offices { get; set; }
    public DbSet<IndexCounter> IndexCounters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========== STUDENT CONFIG ==========
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(s => s.Id);

            // Unique index on UniversityIndex
            entity.HasIndex(s => s.UniversityIndex).IsUnique();

            // Address as an owned entity
            entity.OwnsOne(
                s => s.ResidenceAddress,
                adres =>
                {
                    adres.Property(a => a.Street).HasColumnName("Street");
                    adres.Property(a => a.City).HasColumnName("City");
                    adres.Property(a => a.PostalCode).HasColumnName("PostalCode");
                }
            );
        });

        // ========== MASTER STUDENT CONFIG (INHERITANCE) ==========
        modelBuilder.Entity<MasterStudent>(entity =>
        {
            // Relation to supervisor
            entity
                .HasOne(s => s.Supervisor)
                .WithMany(p => p.SupervisedStudents)
                .HasForeignKey(s => s.SupervisorId)
                .OnDelete(DeleteBehavior.SetNull); // Deleting a professor does not delete the student
        });

        // ========== PROFESSOR CONFIG ==========
        modelBuilder.Entity<Professor>(entity =>
        {
            entity.HasKey(p => p.Id);

            // Unique index on UniversityIndex
            entity.HasIndex(p => p.UniversityIndex).IsUnique();

            // Address as an owned entity
            entity.OwnsOne(
                p => p.ResidenceAddress,
                adres =>
                {
                    adres.Property(a => a.Street).HasColumnName("Street");
                    adres.Property(a => a.City).HasColumnName("City");
                    adres.Property(a => a.PostalCode).HasColumnName("PostalCode");
                }
            );

            // 1:1 relation with Office
            entity
                .HasOne(p => p.Office)
                .WithOne(g => g.Professor)
                .HasForeignKey<Office>(g => g.ProfessorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== OFFICE CONFIG ==========
        modelBuilder.Entity<Office>(entity =>
        {
            entity.HasKey(g => g.Id);
        });

        // ========== COURSE CONFIG ==========
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(k => k.Id);

            // Many-to-one relation with Department
            entity
                .HasOne(k => k.Department)
                .WithMany(w => w.Courses)
                .HasForeignKey(k => k.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-one relation with Professor
            entity
                .HasOne(k => k.Professor)
                .WithMany(p => p.TaughtCourses)
                .HasForeignKey(k => k.ProfessorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Self-referencing many-to-many dla Prerequisites
            entity
                .HasMany(k => k.Prerequisites)
                .WithMany(k => k.RequiredByCourses)
                .UsingEntity<Dictionary<string, object>>(
                    "CoursePrerequisites",
                    j =>
                        j.HasOne<Course>()
                            .WithMany()
                            .HasForeignKey("PrerequisiteId")
                            .OnDelete(DeleteBehavior.Cascade),
                    j =>
                        j.HasOne<Course>()
                            .WithMany()
                            .HasForeignKey("CourseId")
                            .OnDelete(DeleteBehavior.Cascade)
                );
        });

        // ========== DEPARTMENT CONFIG ==========
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(w => w.Id);
        });

        // ========== ENROLLMENT CONFIG ==========
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Many-to-one relation with Student
            entity
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-one relation with Course
            entity
                .HasOne(e => e.Course)
                .WithMany(k => k.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique index - a student may be enrolled in a course only once
            entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
        });

        // ========== INDEX COUNTERS CONFIG ==========
        modelBuilder.Entity<IndexCounter>(entity =>
        {
            entity.HasKey(l => l.Prefix);
        });
    }
}
