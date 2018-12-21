using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace abstractplay.DB
{
    public partial class GenContext : DbContext
    {
        public virtual DbSet<Announcements> Announcements { get; set; }
        public virtual DbSet<Challenges> Challenges { get; set; }
        public virtual DbSet<ChallengesPlayers> ChallengesPlayers { get; set; }
        public virtual DbSet<Consoles> Consoles { get; set; }
        public virtual DbSet<ConsolesVotes> ConsolesVotes { get; set; }
        public virtual DbSet<GamesArchive> GamesArchive { get; set; }
        public virtual DbSet<GamesData> GamesData { get; set; }
        public virtual DbSet<GamesDataChats> GamesDataChats { get; set; }
        public virtual DbSet<GamesDataClocks> GamesDataClocks { get; set; }
        public virtual DbSet<GamesDataPlayers> GamesDataPlayers { get; set; }
        public virtual DbSet<GamesDataStates> GamesDataStates { get; set; }
        public virtual DbSet<GamesDataWhoseturn> GamesDataWhoseturn { get; set; }
        public virtual DbSet<GamesMeta> GamesMeta { get; set; }
        public virtual DbSet<GamesMetaPublishers> GamesMetaPublishers { get; set; }
        public virtual DbSet<GamesMetaRanks> GamesMetaRanks { get; set; }
        public virtual DbSet<GamesMetaStatus> GamesMetaStatus { get; set; }
        public virtual DbSet<GamesMetaTags> GamesMetaTags { get; set; }
        public virtual DbSet<GamesMetaVariants> GamesMetaVariants { get; set; }
        public virtual DbSet<Owners> Owners { get; set; }
        public virtual DbSet<OwnersNames> OwnersNames { get; set; }

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

            modelBuilder.Entity<Challenges>(entity =>
            {
                entity.HasKey(e => e.ChallengeId);

                entity.ToTable("challenges");

                entity.HasIndex(e => e.DateIssued)
                    .HasName("idx_dateissued");

                entity.HasIndex(e => e.GameId)
                    .HasName("idx_gameid");

                entity.HasIndex(e => e.OwnerId)
                    .HasName("idx_ownerid");

                entity.Property(e => e.ChallengeId).HasMaxLength(16);

                entity.Property(e => e.ClockInc).HasDefaultValueSql("'24'");

                entity.Property(e => e.ClockMax).HasDefaultValueSql("'240'");

                entity.Property(e => e.ClockStart).HasDefaultValueSql("'72'");

                entity.Property(e => e.DateIssued)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.GameId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.Notes).HasColumnType("text");

                entity.Property(e => e.NumPlayers).HasDefaultValueSql("'2'");

                entity.Property(e => e.OwnerId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.Variants).HasColumnType("text");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.Challenges)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_challenge2game");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.Challenges)
                    .HasForeignKey(d => d.OwnerId)
                    .HasConstraintName("fk_challenge2issuer");
            });

            modelBuilder.Entity<ChallengesPlayers>(entity =>
            {
                entity.HasKey(e => e.EntryId);

                entity.ToTable("challenges_players");

                entity.HasIndex(e => e.ChallengeId)
                    .HasName("idx_challengeid");

                entity.HasIndex(e => e.Confirmed)
                    .HasName("idx_confirmed");

                entity.HasIndex(e => e.OwnerId)
                    .HasName("idx_ownerid");

                entity.HasIndex(e => new { e.ChallengeId, e.OwnerId })
                    .HasName("idx_uniqueplayers")
                    .IsUnique();

                entity.Property(e => e.EntryId).HasMaxLength(16);

                entity.Property(e => e.ChallengeId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.Confirmed).HasColumnType("bit(1)");

                entity.Property(e => e.OwnerId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.HasOne(d => d.Challenge)
                    .WithMany(p => p.ChallengesPlayers)
                    .HasForeignKey(d => d.ChallengeId)
                    .HasConstraintName("fk_entry2challenge");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.ChallengesPlayers)
                    .HasForeignKey(d => d.OwnerId)
                    .HasConstraintName("fk_entry2owner");
            });

            modelBuilder.Entity<Consoles>(entity =>
            {
                entity.HasKey(e => e.EntryId);

                entity.ToTable("consoles");

                entity.HasIndex(e => e.Command)
                    .HasName("idx_type");

                entity.HasIndex(e => e.OwnerId)
                    .HasName("fk_console2owner");

                entity.HasIndex(e => e.Timestamp)
                    .HasName("idx_date");

                entity.HasIndex(e => new { e.GameId, e.Command })
                    .HasName("idx_uniquegame")
                    .IsUnique();

                entity.Property(e => e.EntryId).HasMaxLength(16);

                entity.Property(e => e.Data).HasColumnType("varchar(255)");

                entity.Property(e => e.GameId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.OwnerId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.Timestamp)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.Consoles)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_console2game");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.Consoles)
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_console2owner");
            });

            modelBuilder.Entity<ConsolesVotes>(entity =>
            {
                entity.HasKey(e => e.EntryId);

                entity.ToTable("consoles_votes");

                entity.HasIndex(e => e.ConsoleId)
                    .HasName("fk_entry2console");

                entity.HasIndex(e => e.Voter)
                    .HasName("fk_voter2owner");

                entity.Property(e => e.EntryId).HasMaxLength(16);

                entity.Property(e => e.ConsoleId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.Vote).HasColumnType("bit(1)");

                entity.Property(e => e.Voter)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.HasOne(d => d.Console)
                    .WithMany(p => p.ConsolesVotes)
                    .HasForeignKey(d => d.ConsoleId)
                    .HasConstraintName("fk_entry2console");

                entity.HasOne(d => d.VoterNavigation)
                    .WithMany(p => p.ConsolesVotes)
                    .HasForeignKey(d => d.Voter)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_voter2owner");
            });

            modelBuilder.Entity<GamesArchive>(entity =>
            {
                entity.HasKey(e => e.ReportId);

                entity.ToTable("games_archive");

                entity.Property(e => e.ReportId).HasMaxLength(16);

                entity.Property(e => e.Json)
                    .IsRequired()
                    .HasColumnType("longtext");
            });

            modelBuilder.Entity<GamesData>(entity =>
            {
                entity.HasKey(e => e.EntryId);

                entity.ToTable("games_data");

                entity.HasIndex(e => e.Alert)
                    .HasName("idx_alert");

                entity.HasIndex(e => e.ClockFrozen)
                    .HasName("idx_frozen");

                entity.HasIndex(e => e.Closed)
                    .HasName("idx_closed");

                entity.HasIndex(e => e.GameMetaId)
                    .HasName("idx_gameid");

                entity.HasIndex(e => e.Variants)
                    .HasName("idx_variants");

                entity.Property(e => e.EntryId).HasMaxLength(16);

                entity.Property(e => e.Alert).HasColumnType("bit(1)");

                entity.Property(e => e.ClockFrozen).HasColumnType("bit(1)");

                entity.Property(e => e.Closed).HasColumnType("bit(1)");

                entity.Property(e => e.GameMetaId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.Variants).HasColumnType("text");

                entity.HasOne(d => d.GameMeta)
                    .WithMany(p => p.GamesData)
                    .HasForeignKey(d => d.GameMetaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_data2meta");
            });

            modelBuilder.Entity<GamesDataChats>(entity =>
            {
                entity.HasKey(e => e.ChatId);

                entity.ToTable("games_data_chats");

                entity.HasIndex(e => e.GameId)
                    .HasName("fk_chat2game");

                entity.HasIndex(e => e.Message)
                    .HasName("idx_msg");

                entity.HasIndex(e => e.OwnerId)
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

                entity.Property(e => e.OwnerId).HasMaxLength(16);

                entity.Property(e => e.Timestamp)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'current_timestamp()'")
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GamesDataChats)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_chat2game");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.GamesDataChats)
                    .HasForeignKey(d => d.OwnerId)
                    .HasConstraintName("fk_chat2owner");
            });

            modelBuilder.Entity<GamesDataClocks>(entity =>
            {
                entity.HasKey(e => new { e.GameId, e.OwnerId });

                entity.ToTable("games_data_clocks");

                entity.HasIndex(e => e.Bank)
                    .HasName("idx_bank");

                entity.HasIndex(e => e.GameId)
                    .HasName("idx_game");

                entity.HasIndex(e => e.OwnerId)
                    .HasName("idx_owner");

                entity.Property(e => e.GameId).HasMaxLength(16);

                entity.Property(e => e.OwnerId).HasMaxLength(16);

                entity.Property(e => e.Bank).HasColumnType("smallint(5)");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GamesDataClocks)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_bank2game");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.GamesDataClocks)
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_bank2owner");
            });

            modelBuilder.Entity<GamesDataPlayers>(entity =>
            {
                entity.HasKey(e => new { e.GameId, e.OwnerId });

                entity.ToTable("games_data_players");

                entity.HasIndex(e => e.GameId)
                    .HasName("idx_gameid");

                entity.HasIndex(e => e.OwnerId)
                    .HasName("fk_player2player");

                entity.Property(e => e.GameId).HasMaxLength(16);

                entity.Property(e => e.OwnerId).HasMaxLength(16);

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GamesDataPlayers)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_player2game");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.GamesDataPlayers)
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_player2owner");
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

                entity.Property(e => e.RenderRep)
                    .IsRequired()
                    .HasColumnType("mediumtext");

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
                entity.HasKey(e => new { e.GameId, e.OwnerId });

                entity.ToTable("games_data_whoseturn");

                entity.HasIndex(e => e.GameId)
                    .HasName("idx_gameid");

                entity.HasIndex(e => e.OwnerId)
                    .HasName("fk_turn2player");

                entity.Property(e => e.GameId).HasMaxLength(16);

                entity.Property(e => e.OwnerId).HasMaxLength(16);

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GamesDataWhoseturn)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_turn2game");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.GamesDataWhoseturn)
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_turn2owner");
            });

            modelBuilder.Entity<GamesMeta>(entity =>
            {
                entity.HasKey(e => e.GameId);

                entity.ToTable("games_meta");

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

                entity.Property(e => e.IsLive).HasColumnType("bit(1)");

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

                entity.Property(e => e.Version)
                    .IsRequired()
                    .HasColumnType("varchar(10)");

                entity.HasOne(d => d.Publisher)
                    .WithMany(p => p.GamesMeta)
                    .HasForeignKey(d => d.PublisherId)
                    .HasConstraintName("fk_publisher2game");
            });

            modelBuilder.Entity<GamesMetaPublishers>(entity =>
            {
                entity.HasKey(e => e.PublisherId);

                entity.ToTable("games_meta_publishers");

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

            modelBuilder.Entity<GamesMetaRanks>(entity =>
            {
                entity.HasKey(e => e.EntryId);

                entity.ToTable("games_meta_ranks");

                entity.HasIndex(e => e.GameId)
                    .HasName("fk_game2game");

                entity.HasIndex(e => e.Rank)
                    .HasName("idx_rank");

                entity.HasIndex(e => new { e.OwnerId, e.GameId })
                    .HasName("idx_unique")
                    .IsUnique();

                entity.Property(e => e.EntryId).HasMaxLength(16);

                entity.Property(e => e.GameId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.OwnerId)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GamesMetaRanks)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_game2game");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.GamesMetaRanks)
                    .HasForeignKey(d => d.OwnerId)
                    .HasConstraintName("fk_owner2owner");
            });

            modelBuilder.Entity<GamesMetaStatus>(entity =>
            {
                entity.HasKey(e => e.StatusId);

                entity.ToTable("games_meta_status");

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

                entity.Property(e => e.IsUp).HasColumnType("bit(1)");

                entity.Property(e => e.Message).HasColumnType("varchar(255)");

                entity.Property(e => e.Timestamp)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("'current_timestamp()'")
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GamesMetaStatus)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_status2game");
            });

            modelBuilder.Entity<GamesMetaTags>(entity =>
            {
                entity.HasKey(e => e.EntryId);

                entity.ToTable("games_meta_tags");

                entity.HasIndex(e => e.GameId)
                    .HasName("fk_gameid_tags");

                entity.HasIndex(e => e.OwnerId)
                    .HasName("fk_userid_tags");

                entity.HasIndex(e => e.Tag)
                    .HasName("idx_tag");

                entity.HasIndex(e => new { e.GameId, e.OwnerId, e.Tag })
                    .HasName("idx_unique")
                    .IsUnique();

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
                    .WithMany(p => p.GamesMetaTags)
                    .HasForeignKey(d => d.GameId)
                    .HasConstraintName("fk_tag2game");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.GamesMetaTags)
                    .HasForeignKey(d => d.OwnerId)
                    .HasConstraintName("fk_tag2owner");
            });

            modelBuilder.Entity<GamesMetaVariants>(entity =>
            {
                entity.HasKey(e => new { e.GameId, e.VariantId });

                entity.ToTable("games_meta_variants");

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
                    .WithMany(p => p.GamesMetaVariants)
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

                entity.Property(e => e.Anonymous).HasColumnType("bit(1)");

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
