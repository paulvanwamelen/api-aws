using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public class ScaffoldingDesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection services)
        {
            var options = ReverseEngineerOptions.DbContextAndEntities;
            services.AddHandlebarsScaffolding(options);
        }
    }

    public partial class MyContext : GenContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string db_server = System.Environment.GetEnvironmentVariable("db_server");
                string db_database = System.Environment.GetEnvironmentVariable("db_database");
                string db_username = System.Environment.GetEnvironmentVariable("db_username");
                string db_password = System.Environment.GetEnvironmentVariable("db_password");
                optionsBuilder
                	.UseMySql("Server=" + db_server + ";database=" + db_database + ";uid=" + db_username + ";pwd=" + db_password + ";")
                    .ReplaceService<IComparer<ModificationCommand>, MyModificationCommandComparer>()
                	.UseLazyLoadingProxies();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            // var byteComparer = new ValueComparer<byte[]>(
            //     (p1, p2) => string.Concat(p1.Select(i => string.Format("{0:x2}", i))).Equals(string.Concat(p2.Select(i => string.Format("{0:x2}", i)))),
            //     p => p != null ? p.GetHashCode() : 0,
            //     p => p != null ? (byte[])p.Clone() : default
            // );

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    //Value Converters
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(dateTimeConverter);

                    //Comparers
                    // if (property.ClrType == typeof(byte[]))
                    //     property.SetValueComparer(byteComparer);
                }
            }
        }

        public void RejectChanges()
        {
            foreach (var entry in ChangeTracker.Entries().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified; //Revert changes made to deleted entity.
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                }
            }
        }
    }
}