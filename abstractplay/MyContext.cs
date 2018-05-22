using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace abstractplay
{
    [Table("owners")]
    public class Owner
    {
        [Key]
        public byte[] OwnerId { get; set; }
        public byte[] CognitoId { get; set; }
        public byte[] PlayerId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime ConsentDate { get; set; }
        public string Country { get; set; }
        public bool Anonymous { get; set; }
        public string Tagline { get; set; }

        public ICollection<NameEntry> Names { get; set; }
    }

    [Table("owners_names")]
    public class NameEntry
    {
        [Key]
        public byte[] EntryId { get; set; }
        [ForeignKey("Owner")]
        public byte[] OwnerId { get; set; }
        public string Name { get; set; }
        public DateTime EffectiveFrom { get; set; }
    }

    [Table("announcements")]
    public class Announcement
    {
        [Key]
        public DateTime DatePosted { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
    }

    public class MyContext: DbContext
    {
        public MyContext() : base()
        {
        }

        public DbSet<Owner> Owners { get; set; }
        public DbSet<Announcement> Announcements { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string db_server = System.Environment.GetEnvironmentVariable("db_server");
            Debug.Assert(!String.IsNullOrWhiteSpace(db_server));
            string db_database = System.Environment.GetEnvironmentVariable("db_database");
            Debug.Assert(!String.IsNullOrWhiteSpace(db_database));
            string db_username = System.Environment.GetEnvironmentVariable("db_username");
            Debug.Assert(!String.IsNullOrWhiteSpace(db_username));
            string db_password = System.Environment.GetEnvironmentVariable("db_password");
            Debug.Assert(!String.IsNullOrWhiteSpace(db_password));
            optionsBuilder
            .UseMySql("Server=" + db_server + ";database=" + db_database + ";uid=" + db_username + ";pwd=" + db_password + ";");
        }
    }
}
