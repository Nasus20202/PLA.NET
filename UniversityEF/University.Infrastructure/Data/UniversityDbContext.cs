using Microsoft.EntityFrameworkCore;
using University.Domain.Entities;

namespace University.Infrastructure.Data;

/// <summary>
/// Kontekst bazy danych dla systemu uniwersyteckiego
/// </summary>
public class UniversityDbContext : DbContext
{
    public UniversityDbContext(DbContextOptions<UniversityDbContext> options) : base(options)
    {
    }

    // DbSets dla wszystkich encji
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

        // ========== KONFIGURACJA STUDENT ==========
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(s => s.Id);
            
            // Unikalny indeks na UniversityIndex
            entity.HasIndex(s => s.UniversityIndex)
                .IsUnique();
            
            // Address jako Owned Entity
            entity.OwnsOne(s => s.ResidenceAddress, adres =>
            {
                adres.Property(a => a.Street).HasColumnName("Street");
                adres.Property(a => a.City).HasColumnName("City");
                adres.Property(a => a.PostalCode).HasColumnName("PostalCode");
            });
        });

        // ========== KONFIGURACJA STUDENT STUDIÓW MAGISTERSKICH (DZIEDZICZENIE) ==========
        modelBuilder.Entity<MasterStudent>(entity =>
        {
            // Relacja do promotora
            entity.HasOne(s => s.Supervisor)
                .WithMany(p => p.SupervisedStudents)
                .HasForeignKey(s => s.SupervisorId)
                .OnDelete(DeleteBehavior.SetNull); // Usunięcie profesora nie usuwa studenta
        });

        // ========== KONFIGURACJA PROFESOR ==========
        modelBuilder.Entity<Professor>(entity =>
        {
            entity.HasKey(p => p.Id);
            
            // Unikalny indeks na UniversityIndex
            entity.HasIndex(p => p.UniversityIndex)
                .IsUnique();
            
            // Address jako Owned Entity
            entity.OwnsOne(p => p.ResidenceAddress, adres =>
            {
                adres.Property(a => a.Street).HasColumnName("Street");
                adres.Property(a => a.City).HasColumnName("City");
                adres.Property(a => a.PostalCode).HasColumnName("PostalCode");
            });
            
            // Relacja 1:1 z Office
            entity.HasOne(p => p.Office)
                .WithOne(g => g.Professor)
                .HasForeignKey<Office>(g => g.ProfessorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== KONFIGURACJA GABINET ==========
        modelBuilder.Entity<Office>(entity =>
        {
            entity.HasKey(g => g.Id);
        });

        // ========== KONFIGURACJA KURS ==========
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(k => k.Id);
            
            // Relacja wiele-do-jednego z Wydziałem
            entity.HasOne(k => k.Department)
                .WithMany(w => w.Courses)
                .HasForeignKey(k => k.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Relacja wiele-do-jednego z Professorem
            entity.HasOne(k => k.Professor)
                .WithMany(p => p.TaughtCourses)
                .HasForeignKey(k => k.ProfessorId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Self-referencing many-to-many dla Prerequisites
            entity.HasMany(k => k.Prerequisites)
                .WithMany(k => k.RequiredByCourses)
                .UsingEntity<Dictionary<string, object>>(
                    "CoursePrerequisites",
                    j => j.HasOne<Course>()
                        .WithMany()
                        .HasForeignKey("PrerequisiteId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<Course>()
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                );
        });

        // ========== KONFIGURACJA WYDZIAŁ ==========
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(w => w.Id);
        });

        // ========== KONFIGURACJA ENROLLMENT ==========
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Relacja wiele-do-jednego ze Studentem
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Relacja wiele-do-jednego z Courseem
            entity.HasOne(e => e.Course)
                .WithMany(k => k.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Unikalny indeks - student może być zapisany na kurs tylko raz
            entity.HasIndex(e => new { e.StudentId, e.CourseId })
                .IsUnique();
        });

        // ========== KONFIGURACJA LICZNIK INDEKSÓW ==========
        modelBuilder.Entity<IndexCounter>(entity =>
        {
            entity.HasKey(l => l.Prefix);
        });
    }
}
