using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.routes.Domain.Model.Entities;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration.Extensions;
//AGREGATES
using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Model.Aggregates.Geographic;
using Frock_backend.transport_Company.Domain.Model.Aggregates;
using Frock_backend.IAM.Domain.Model.Aggregates;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Reflection.Emit;

using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.AddCreatedUpdatedInterceptor();
            base.OnConfiguring(builder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // IAM Context
            builder.Entity<User>().HasKey(u => u.Id);
            builder.Entity<User>().Property(u => u.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<User>().Property(u => u.Username).IsRequired();
            builder.Entity<User>().Property(u => u.PasswordHash).IsRequired();
            builder.Entity<User>().Property(u => u.Role).HasConversion<string>().IsRequired();

            //COMPANY
            builder.Entity<Company>().HasKey(f => f.Id);
            builder.Entity<Company>().Property(f => f.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<Company>().Property(f => f.Name).IsRequired();
            builder.Entity<Company>().Property(f => f.LogoUrl).IsRequired();
            // Fix for CS1660: The issue arises because the lambda expression is being used in a context where a string is expected.
            // The correct fix is to specify the type explicitly for the foreign key property.
            builder.Entity<Company>()
                .HasOne<User>() // Una Company tiene un User (dueño)
                .WithOne() // Un User puede tener una Company (dueño)
                .HasForeignKey<Company>(c => c.FkIdUser) // Specify the entity type explicitly
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            //REGION
            builder.Entity<Region>().HasKey(f => f.Id);
            builder.Entity<Region>().Property(f => f.Id).IsRequired(); // no se pone value generated on add porque eso lo maneja el seeder, son valores estaticos
            builder.Entity<Region>().Property(f => f.Name).IsRequired();

            //PROVINCE
            builder.Entity<Province>().HasKey(f => f.Id);
            builder.Entity<Province>().Property(f => f.Id).IsRequired();
            builder.Entity<Province>().Property(f => f.Name).IsRequired();
            builder.Entity<Province>()
                .HasOne<Region>()
                .WithMany()
                .HasForeignKey(p => p.FkIdRegion)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            //DISTRICT
            builder.Entity<District>().HasKey(f => f.Id);
            builder.Entity<District>().Property(f => f.Id).IsRequired();
            builder.Entity<District>().Property(f => f.Name).IsRequired();
            builder.Entity<District>()
                .HasOne<Province>()
                .WithMany()
                .HasForeignKey(d => d.FkIdProvince)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            //STOP
            builder.Entity<Stop>().HasKey(f => f.Id);
            builder.Entity<Stop>().Property(f => f.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<Stop>().Property(f => f.Name).IsRequired();
            builder.Entity<Stop>().Property(f => f.GoogleMapsUrl).IsRequired();
            builder.Entity<Stop>().Property(f => f.ImageUrl).IsRequired();
            builder.Entity<Stop>().Property(f => f.Phone).IsRequired();
            builder.Entity<Stop>().Property(f => f.Address).IsRequired();
            builder.Entity<Stop>().Property(f => f.Reference).IsRequired();

            builder.Entity<Stop>()
                .HasOne<Company>() // Un Stop tiene una Company
                .WithMany() // Una Company tiene muchos Stops
                .HasForeignKey(l => l.FkIdCompany)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Stop>()
                .HasOne<District>() // Un Stop tiene una District
                .WithMany() // Una District tiene muchos Stops
                .HasForeignKey(f => f.FkIdDistrict)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // ROUTE
            builder.Entity<RouteAggregate>(b =>
            {
                b.ToTable("Routes");
                b.HasKey(r => r.Id);
                b.Property(r => r.Price).IsRequired();
                b.Property(r => r.Duration).IsRequired();
                b.Property(r => r.Frequency).IsRequired();
                // 2) Schedule como entidad hija (1-N)
                b.HasMany(r => r.Schedules)
                 .WithOne()                               // si no navegas hacia RouteAggregate
                 .HasForeignKey(s => s.RouteId)
                 .OnDelete(DeleteBehavior.Cascade);
                // Owned type: RouteStop → tabla intermedia
                // RouteStops (join table)
                builder.Entity<RoutesStops>(b =>
                {
                    b.ToTable("RouteStops");
                    // clave compuesta RouteId+StopId
                    b.HasKey(rs => rs.Id);

                    // relación a RouteAggregate
                    b.HasOne(rs => rs.Route)
                     .WithMany(r => r.Stops)
                     .HasForeignKey(rs => rs.FKRouteId);

                    // relación a Stop
                    b.HasOne(rs => rs.Stop)
                     .WithMany()
                     .HasForeignKey(rs => rs.FkStopId)
                     .OnDelete(DeleteBehavior.Restrict);
                });
            }
            );
            // 4) Schedule
            builder.Entity<Schedule>(b =>
            {
                b.ToTable("Schedules");
                b.HasKey(s => s.Id);
                b.Property(s => s.StartTime).IsRequired();
                b.Property(s => s.EndTime).IsRequired();
                b.Property(s => s.DayOfWeek).HasMaxLength(10);
            });


            builder.UseSnakeCaseNamingConvention();
            //just a comment to commit because i forgot to add this line in the last commit

            //now we use the correct naming convention for the database, it used to be in comments so thats why it was not working
        }
    }
}
