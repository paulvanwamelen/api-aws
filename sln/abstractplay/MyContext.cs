using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

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
                	.UseLazyLoadingProxies();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(dateTimeConverter);
                }
            }
        }
    }
}