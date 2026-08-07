using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RslLibrary.Models;

namespace RslLibrary.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<CopyStatus> CopyStatuses { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<BookCopy> Copies { get; set; }
    public DbSet<LoanStatus> LoanStatuses { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<ReservationStatus> ReservationStatuses { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Author>().ToTable("authors");
        modelBuilder.Entity<Publisher>().ToTable("publishers");
        modelBuilder.Entity<Category>().ToTable("categories");
        modelBuilder.Entity<Language>().ToTable("languages");
        modelBuilder.Entity<Book>().ToTable("books");
        modelBuilder.Entity<CopyStatus>().ToTable("copy_statuses");
        modelBuilder.Entity<Location>().ToTable("locations");
        modelBuilder.Entity<BookCopy>().ToTable("copies");
        modelBuilder.Entity<LoanStatus>().ToTable("loan_statuses");
        modelBuilder.Entity<Loan>().ToTable("loans");
        modelBuilder.Entity<ReservationStatus>().ToTable("reservation_statuses");
        modelBuilder.Entity<Reservation>().ToTable("reservations");
        modelBuilder.Entity<AuditLog>().ToTable("audit_logs");

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasOne(b => b.Author).WithMany(a => a.Books).HasForeignKey(b => b.AuthorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(b => b.Category).WithMany(c => c.Books).HasForeignKey(b => b.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(b => b.Publisher).WithMany(p => p.Books).HasForeignKey(b => b.PublisherId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookCopy>(entity =>
        {
            entity.HasOne(c => c.Book).WithMany(b => b.Copies).HasForeignKey(c => c.BookId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(c => c.Status).WithMany(s => s.Copies).HasForeignKey(c => c.StatusId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(c => c.InventoryNumber).IsUnique();
            entity.HasIndex(c => c.Barcode).IsUnique();
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasOne(l => l.Copy).WithMany(c => c.Loans).HasForeignKey(l => l.CopyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(l => l.User).WithMany(u => u.Loans).HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(l => l.Status).WithMany(s => s.Loans).HasForeignKey(l => l.StatusId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasOne(r => r.Copy).WithMany(c => c.Reservations).HasForeignKey(r => r.CopyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.User).WithMany(u => u.Reservations).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Status).WithMany(s => s.Reservations).HasForeignKey(r => r.StatusId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.Property(a => a.OldValues).HasColumnType("jsonb");
            entity.Property(a => a.NewValues).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Book>().HasQueryFilter(b => !b.IsDeleted);
        modelBuilder.Entity<BookCopy>().HasQueryFilter(c => !c.IsDeleted);
    }
}