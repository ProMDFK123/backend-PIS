using bolsafeucn_back.src.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace bolsafeucn_back.src.Infrastructure.Data
{
    /// <summary>
    /// Contexto de base de datos principal de la aplicación
    /// Hereda de IdentityDbContext para incluir las funcionalidades de autenticación
    /// </summary>
    public class AppDbContext : IdentityDbContext<GeneralUser, Role, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // DbSets - Representan las tablas en la base de datos
        public DbSet<Image> Images { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Individual> Individuals { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<BuySell> BuySells { get; set; }
        public DbSet<NotificationDTO> Notifications { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<Review> Reviews { get; set; }

        // public DbSet<Review> Reviews { get; set; } // Desactivado temporalmente

        /// <summary>
        /// Configura las relaciones entre entidades y otras configuraciones de EF Core
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relaciones uno a uno entre GeneralUser y tipos específicos de usuario

            // Relación Student - Un usuario puede ser un estudiante
            builder
                .Entity<Student>()
                .HasOne(s => s.GeneralUser)
                .WithOne(gu => gu.Student)
                .HasForeignKey<Student>(s => s.GeneralUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Company - Un usuario puede ser una empresa
            builder
                .Entity<Company>()
                .HasOne(c => c.GeneralUser)
                .WithOne(gu => gu.Company)
                .HasForeignKey<Company>(c => c.GeneralUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Admin - Un usuario puede ser un administrador
            builder
                .Entity<Admin>()
                .HasOne(a => a.GeneralUser)
                .WithOne(gu => gu.Admin)
                .HasForeignKey<Admin>(a => a.GeneralUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Individual - Un usuario puede ser un particular
            builder
                .Entity<Individual>()
                .HasOne(i => i.GeneralUser)
                .WithOne(gu => gu.Individual)
                .HasForeignKey<Individual>(i => i.GeneralUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relaciones de JobApplication (Postulación a oferta)
            // Un estudiante puede hacer muchas postulaciones
            builder
                .Entity<JobApplication>()
                .HasOne(ja => ja.Student)
                .WithMany()
                .HasForeignKey(ja => ja.StudentId);

            // Una oferta puede tener muchas postulaciones
            builder
                .Entity<JobApplication>()
                .HasOne(ja => ja.JobOffer)
                .WithMany()
                .HasForeignKey(ja => ja.JobOfferId);

            // Relaciones de Publication (clase base para ofertas y compra/venta)
            // Un usuario puede crear muchas publicaciones
            builder
                .Entity<Publication>()
                .HasOne(p => p.User)
                .WithMany(u => u.Publications)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relaciones de Review
            
            // Review - Publication (1:1)
            // Una publicación puede tener una review
            builder
                .Entity<Review>()
                .HasOne(r => r.Publication)
                .WithOne()
                .HasForeignKey<Review>(r => r.PublicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review - Student (Many:1)
            // Un estudiante puede tener muchas reviews (como estudiante evaluado)
            builder
                .Entity<Review>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review - Offeror (Many:1)
            // Un proveedor puede tener muchas reviews (como oferente evaluado)
            builder
                .Entity<Review>()
                .HasOne(r => r.Offeror)
                .WithMany()
                .HasForeignKey(r => r.OfferorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciones de UserImage (Imágenes de usuario)
            // Relación uno a uno entre GeneralUser y sus imágenes de perfil y banner
            builder
                .Entity<GeneralUser>()
                .HasOne(gu => gu.ProfilePhoto)
                .WithOne()
                .HasForeignKey<GeneralUser>(gu => gu.ProfilePhotoId)
                .OnDelete(DeleteBehavior.SetNull);
            builder
                .Entity<GeneralUser>()
                .HasOne(gu => gu.ProfileBanner)
                .WithOne()
                .HasForeignKey<GeneralUser>(gu => gu.ProfileBannerId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
    // todo: sobrecarga de SaveChangesAsync para manejar IsCompleted en Review.
    
}
