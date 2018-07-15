using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

    public partial class MyContext : DbContext
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
                	.UseMySql("Server=" + db_server + ";database=" + db_database + ";uid=" + db_username + ";pwd=" + db_password + ";TreatTinyAsBoolean=false")
                	.UseLazyLoadingProxies();
            }
        }
    }
}