using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using api.cabcheap.com.Models;

namespace api.cabcheap.com.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

             //Relations

            builder.Entity<ApplicationUser>()
                .HasIndex("ProviderId")
                .IsUnique()
                .HasName("ProviderIdIndex");
            
            builder.Entity<ApplicationUser>()
                .HasIndex("UserName")
                .HasName("NonNormalizedUserNameIndex");

            builder.Entity<ApplicationUser>()
                .HasIndex("ProviderName")
                .HasName("ProviderNameIndex");
            //can make forieg key later

            /* builder.Entity<Image>()
                .Property(i => i.FullImage)
                .HasColumnType("MediumBlob");

            builder.Entity<Image>()
                .Property(i => i.ThumbnailImage)
                .HasColumnType("Blob");


            builder.Entity<Post>()
                .HasOne(p => p.Image);

            builder.Entity<Comment>()
                .HasOne(c => c.Image);

            builder.Entity<Post>()
                .HasMany(p => p.Comments);
            
            builder.Entity<Comment>()
                .HasOne(c => c.Post); */
        }
    }
}
