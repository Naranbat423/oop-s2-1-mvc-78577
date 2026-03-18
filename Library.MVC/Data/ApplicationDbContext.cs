using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Library.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Library.MVC.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Member> Members { get; set; } = null!;
    public DbSet<Loan> Loans { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Loan>()
            .HasOne(l => l.Book)
            .WithMany(b => b.Loans)
            .HasForeignKey(l => l.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Loan>()
            .HasOne(l => l.Member)
            .WithMany(m => m.Loans)
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Loan>()
            .HasIndex(l => new { l.BookId, l.ReturnedDate })
            .HasFilter("[ReturnedDate] IS NULL")
            .IsUnique();

        // Seed Admin role and user
        string adminRoleId = Guid.NewGuid().ToString();
        string adminUserId = Guid.NewGuid().ToString();

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN"
            }
        );

        var hasher = new PasswordHasher<IdentityUser>();
        builder.Entity<IdentityUser>().HasData(
            new IdentityUser
            {
                Id = adminUserId,
                UserName = "admin@library.com",
                NormalizedUserName = "ADMIN@LIBRARY.COM",
                Email = "admin@library.com",
                NormalizedEmail = "ADMIN@LIBRARY.COM",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null!, "Admin123!"),
                SecurityStamp = Guid.NewGuid().ToString()
            }
        );

        builder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                RoleId = adminRoleId,
                UserId = adminUserId
            }
        );
    }
}