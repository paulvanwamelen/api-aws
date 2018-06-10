using System;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace abstractplay.DB
{

    public class MyEntityMaterializerSource : EntityMaterializerSource
    {
        private static readonly MethodInfo NormalizeMethod = typeof(DateTimeMapper).GetTypeInfo().GetMethod(nameof(DateTimeMapper.Normalize));

        public override Expression CreateReadValueExpression(Expression valueBuffer, Type type, int index, IProperty property = null)
        {
            if (type == typeof(DateTime))
            {
                return Expression.Call(
                    NormalizeMethod,
                    base.CreateReadValueExpression(valueBuffer, type, index, property)
                );
            }

            return base.CreateReadValueExpression(valueBuffer, type, index, property);
        }
    }

    public static class DateTimeMapper
    {
        public static DateTime Normalize(DateTime value)
        {
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
    }

    public partial class MyContext : DbContext
    {
        public MyContext()
        {
        }

        public MyContext(DbContextOptions<MyContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Announcements> Announcements { get; set; }
        public virtual DbSet<GamesData> GamesData { get; set; }
        public virtual DbSet<GamesDataChats> GamesDataChats { get; set; }
        public virtual DbSet<GamesDataClocks> GamesDataClocks { get; set; }
        public virtual DbSet<GamesDataPlayers> GamesDataPlayers { get; set; }
        public virtual DbSet<GamesDataStates> GamesDataStates { get; set; }
        public virtual DbSet<GamesDataWhoseturn> GamesDataWhoseturn { get; set; }
        public virtual DbSet<GamesInfo> GamesInfo { get; set; }
        public virtual DbSet<GamesInfoPublishers> GamesInfoPublishers { get; set; }
        public virtual DbSet<GamesInfoStatus> GamesInfoStatus { get; set; }
        public virtual DbSet<GamesInfoTags> GamesInfoTags { get; set; }
        public virtual DbSet<GamesInfoVariants> GamesInfoVariants { get; set; }
        public virtual DbSet<Owners> Owners { get; set; }
        public virtual DbSet<OwnersNames> OwnersNames { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string db_server = System.Environment.GetEnvironmentVariable("db_server");
                string db_database = System.Environment.GetEnvironmentVariable("db_database");
                string db_username = System.Environment.GetEnvironmentVariable("db_username");
                string db_password = System.Environment.GetEnvironmentVariable("db_password");
                optionsBuilder
                .UseMySql("Server=" + db_server + ";database=" + db_database + ";uid=" + db_username + ";pwd=" + db_password + ";");
                optionsBuilder.ReplaceService<IEntityMaterializerSource, MyEntityMaterializerSource>();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Announcements>(entity =>
            {
                entity.HasKey(e => e.DatePosted);

                entity.ToTable("announcements");

                entity.Property(e => e.DatePosted).HasColumnType("datetime");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<GamesData>(entity =>
            {
                entity.HasKey(e => e.EntryId);

                entity.ToTable("games_data");

                entity.HasIndex(e => e.Alert)
                    .HasName("idx_alert");

                entity.HasIndex(e => e.Closed)
                    .HasName("idx_closed");

                entity.HasIndex(e => e.Variants)
                    .HasName("idx_variants");

                entity.Property(e => e.EntryId).HasMaxLength(16);

                entity.Property(e => e.Alert)
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Closed)
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.GameId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.Variants).HasColumnType("text");
            });

            modelBuilder.Entity<GamesDataChats>(entity =>
            {
                entity.HasKey(e => e.ChatId);

                entity.ToTable("games_data_chats");

                entity.HasIndex(e => e.GameId)
                    .HasName("fk_chat2game");

                entity.HasIndex(e => e.Message)
                    .HasName("idx_msg");

                entity.HasIndex(e => e.PlayerId)
                    .HasName("fk_chat2player");

                entity.HasIndex(e => e.Timestamp)
                    .HasName("idx_timestamp");

                entity.Property(e => e.ChatId)
                    .HasColumnName("ChatID")
                    .HasMaxLength(16);

                entity.Property(e => e.GameId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.PlayerId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.Timestamp)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'current_timestamp()'")
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GamesDataChats)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_chat2game");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.GamesDataChats)
                    .HasPrincipalKey(p => p.PlayerId)
                    .HasForeignKey(d => d.PlayerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_chat2player");
            });

            modelBuilder.Entity<GamesDataClocks>(entity =>
            {
                entity.HasKey(e => new { e.GameId, e.PlayerId });

                entity.ToTable("games_data_clocks");

                entity.Property(e => e.GameId).HasMaxLength(16);

                entity.Property(e => e.PlayerId).HasMaxLength(16);

                entity.Property(e => e.Current).HasColumnType("int(11)");

                entity.Property(e => e.Increment).HasColumnType("int(11)");

                entity.Property(e => e.Maximum).HasColumnType("int(11)");
            });

            modelBuilder.Entity<GamesDataPlayers>(entity =>
            {
                entity.HasKey(e => new { e.GameId, e.PlayerId });

                entity.ToTable("games_data_players");

                entity.HasIndex(e => e.PlayerId)
                    .HasName("fk_player2player");

                entity.HasIndex(e => e.Seat)
                    .HasName("idx_seat");

                entity.Property(e => e.GameId).HasMaxLength(16);

                entity.Property(e => e.PlayerId).HasMaxLength(16);

                entity.Property(e => e.Seat).HasColumnType("int(11)");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GamesDataPlayers)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_player2game");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.GamesDataPlayers)
                    .HasPrincipalKey(p => p.PlayerId)
                    .HasForeignKey(d => d.PlayerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_player2player");
            });

            modelBuilder.Entity<GamesDataStates>(entity =>
            {
                entity.HasKey(e => e.StateId);

                entity.ToTable("games_data_states");

                entity.HasIndex(e => e.GameId)
                    .HasName("fk_state2game");

                entity.HasIndex(e => e.Timestamp)
                    .HasName("idx_timestamp");

                entity.Property(e => e.StateId).HasMaxLength(16);

                entity.Property(e => e.GameId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.State)
                    .IsRequired()
                    .HasColumnType("mediumtext");

                entity.Property(e => e.Timestamp)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'current_timestamp()'")
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GamesDataStates)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_state2game");
            });

            modelBuilder.Entity<GamesDataWhoseturn>(entity =>
            {
                entity.HasKey(e => new { e.GameId, e.PlayerId });

                entity.ToTable("games_data_whoseturn");

                entity.HasIndex(e => e.PlayerId)
                    .HasName("fk_turn2player");

                entity.Property(e => e.GameId).HasMaxLength(16);

                entity.Property(e => e.PlayerId).HasMaxLength(16);

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GamesDataWhoseturn)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_turn2game");

                entity.HasOne(d => d.Player)
                    .WithMany(p => p.GamesDataWhoseturn)
                    .HasPrincipalKey(p => p.PlayerId)
                    .HasForeignKey(d => d.PlayerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_turn2player");
            });

            modelBuilder.Entity<GamesInfo>(entity =>
            {
                entity.HasKey(e => e.GameId);

                entity.ToTable("games_info");

                entity.HasIndex(e => e.Changelog)
                    .HasName("idx_changelog");

                entity.HasIndex(e => e.Description)
                    .HasName("idx_description");

                entity.HasIndex(e => e.IsLive)
                    .HasName("idx_istesting");

                entity.HasIndex(e => e.LiveDate)
                    .HasName("idx_livedate");

                entity.HasIndex(e => e.Name)
                    .HasName("idx_name")
                    .IsUnique();

                entity.HasIndex(e => e.PlayerCounts)
                    .HasName("idx_playercounts");

                entity.HasIndex(e => e.PublisherId)
                    .HasName("idx_publisher");

                entity.HasIndex(e => e.Rating)
                    .HasName("idx_rating");

                entity.HasIndex(e => e.Shortcode)
                    .HasName("idx_shortcode")
                    .IsUnique();

                entity.HasIndex(e => e.State)
                    .HasName("idx_state");

                entity.HasIndex(e => e.Url)
                    .HasName("idx_URL");

                entity.HasIndex(e => e.Version)
                    .HasName("idx_version");

                entity.Property(e => e.GameId).HasMaxLength(16);

                entity.Property(e => e.Changelog).HasColumnType("text");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.IsLive)
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.LiveDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.PlayerCounts)
                    .IsRequired()
                    .HasColumnType("varchar(255)")
                    .HasDefaultValueSql("'2'");

                entity.Property(e => e.PublisherId).HasMaxLength(16);

                entity.Property(e => e.Shortcode)
                    .IsRequired()
                    .HasColumnType("varchar(25)");

                entity.Property(e => e.State).HasColumnType("varchar(255)");

                entity.Property(e => e.Url).HasColumnType("varchar(255)");

                entity.Property(e => e.Version).HasDefaultValueSql("'1'");

                entity.HasOne(d => d.Publisher)
                    .WithMany(p => p.GamesInfo)
                    .HasForeignKey(d => d.PublisherId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("fk_publisher2game");
            });

            modelBuilder.Entity<GamesInfoPublishers>(entity =>
            {
                entity.HasKey(e => e.PublisherId);

                entity.ToTable("games_info_publishers");

                entity.HasIndex(e => e.EmailAdmin)
                    .HasName("idx_email_admin");

                entity.HasIndex(e => e.EmailTechnical)
                    .HasName("idx_email_tech");

                entity.HasIndex(e => e.Name)
                    .HasName("idx_name")
                    .IsUnique();

                entity.HasIndex(e => e.Url)
                    .HasName("idx_url");

                entity.Property(e => e.PublisherId).HasMaxLength(16);

                entity.Property(e => e.EmailAdmin).HasColumnType("varchar(255)");

                entity.Property(e => e.EmailTechnical).HasColumnType("varchar(255)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Url).HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<GamesInfoStatus>(entity =>
            {
                entity.HasKey(e => e.StatusId);

                entity.ToTable("games_info_status");

                entity.HasIndex(e => e.GameId)
                    .HasName("idx_gameid");

                entity.HasIndex(e => e.IsUp)
                    .HasName("idx_isup");

                entity.HasIndex(e => e.Message)
                    .HasName("idx_msg");

                entity.HasIndex(e => e.Timestamp)
                    .HasName("idx_timestamp");

                entity.Property(e => e.StatusId).HasMaxLength(16);

                entity.Property(e => e.GameId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.IsUp).HasColumnType("tinyint(1)");

                entity.Property(e => e.Message).HasColumnType("varchar(255)");

                entity.Property(e => e.Timestamp)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'current_timestamp()'")
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GamesInfoStatus)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_status2game");
            });

            modelBuilder.Entity<GamesInfoTags>(entity =>
            {
                entity.HasKey(e => e.EntryId);

                entity.ToTable("games_info_tags");

                entity.HasIndex(e => e.GameId)
                    .HasName("fk_gameid_tags");

                entity.HasIndex(e => e.OwnerId)
                    .HasName("fk_userid_tags");

                entity.HasIndex(e => e.Tag)
                    .HasName("idx_tag");

                entity.Property(e => e.EntryId).HasMaxLength(16);

                entity.Property(e => e.GameId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.OwnerId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.Tag)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GamesInfoTags)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_tag2game");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.GamesInfoTags)
                    .HasForeignKey(d => d.OwnerId)
                    .HasConstraintName("fk_tag2owner");
            });

            modelBuilder.Entity<GamesInfoVariants>(entity =>
            {
                entity.HasKey(e => new { e.GameId, e.VariantId });

                entity.ToTable("games_info_variants");

                entity.HasIndex(e => e.Group)
                    .HasName("idx_group");

                entity.HasIndex(e => e.Name)
                    .HasName("idx_name");

                entity.HasIndex(e => e.Note)
                    .HasName("idx_note");

                entity.HasIndex(e => new { e.GameId, e.Name })
                    .HasName("idx_game+name");

                entity.Property(e => e.GameId).HasMaxLength(16);

                entity.Property(e => e.VariantId).HasMaxLength(16);

                entity.Property(e => e.Group).HasColumnType("varchar(255)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Note).HasColumnType("varchar(255)");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GamesInfoVariants)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_var2game");
            });

            modelBuilder.Entity<Owners>(entity =>
            {
                entity.HasKey(e => e.OwnerId);

                entity.ToTable("owners");

                entity.HasIndex(e => e.Anonymous)
                    .HasName("idx_anonymous");

                entity.HasIndex(e => e.CognitoId)
                    .HasName("idx_cognitoid")
                    .IsUnique();

                entity.HasIndex(e => e.Country)
                    .HasName("idx_country");

                entity.HasIndex(e => e.DateCreated)
                    .HasName("idx_created");

                entity.HasIndex(e => e.PlayerId)
                    .HasName("idx_playerid")
                    .IsUnique();

                entity.Property(e => e.OwnerId).HasMaxLength(16);

                entity.Property(e => e.Anonymous)
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.CognitoId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.ConsentDate).HasColumnType("datetime");

                entity.Property(e => e.Country).HasColumnType("char(2)");

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.PlayerId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.Tagline).HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<OwnersNames>(entity =>
            {
                entity.HasKey(e => e.EntryId);

                entity.ToTable("owners_names");

                entity.HasIndex(e => e.EffectiveFrom)
                    .HasName("idx_date");

                entity.HasIndex(e => e.Name)
                    .HasName("idx_name")
                    .IsUnique();

                entity.HasIndex(e => e.OwnerId)
                    .HasName("fk_name2owner");

                entity.Property(e => e.EntryId).HasMaxLength(16);

                entity.Property(e => e.EffectiveFrom).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(30)");

                entity.Property(e => e.OwnerId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.OwnersNames)
                    .HasForeignKey(d => d.OwnerId)
                    .HasConstraintName("fk_name2owner");
            });
        }
    }
}
