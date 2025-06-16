using Microsoft.EntityFrameworkCore;
using TutorialProjectAPI.Models;

namespace TutorialProjectAPI.Contexts          // <- must match all `using ...Contexts`
{
    public class MainContext : DbContext
    {
        public MainContext(DbContextOptions<MainContext> options) : base(options) { }

        public DbSet<UserDB> Users { get; set; }
        public DbSet<PostDB> Posts { get; set; }
        public DbSet<ReplyDB> Replies { get; set; }

        public DbSet<ImageDB> Images { get; set; }
        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<PostDB>()
               .HasOne(p => p.User)
               .WithMany()
               .HasForeignKey(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            b.Entity<ReplyDB>()
               .HasOne(r => r.Post)
               .WithMany(p => p.Replies)
               .HasForeignKey(r => r.PostId)
               .OnDelete(DeleteBehavior.Cascade);

            b.Entity<ReplyDB>()
               .HasOne(r => r.User)
               .WithMany()
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
