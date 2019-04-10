namespace DataModel
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DbModel : DbContext
    {
        private const string connection = @"data source=sportsplay-server.database.windows.net;initial catalog=sportsplay-db;user id=tka4off;password=1Azure_Tkachev!;MultipleActiveResultSets=True;App=EntityFramework";

        public DbModel() : base(connection)
        {
        }

        public virtual DbSet<Meet> Meets { get; set; }
        public virtual DbSet<Playground> Playgrounds { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Meet>()
                .HasMany(e => e.Partakers)
                .WithMany(e => e.TakenMeets)
                .Map(m => m.ToTable("Match").MapLeftKey("MeetId").MapRightKey("PartakerId"));

            modelBuilder.Entity<User>()
                .HasMany(e => e.CreatedMeets)
                .WithRequired(e => e.Founder)
                .HasForeignKey(e => e.FounderId);
        }
    }
}
