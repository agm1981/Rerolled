using System.Data.Entity;
using System.Linq;

namespace Common
{
    public partial class DenormalizeContext : DbContext
    {
        public DenormalizeContext()
            : base("name=Denormalize")
        {

        }

        public virtual DbSet<Post> Posts
        {
            get; set;
        }

        public virtual DbSet<Thread> Threads
        {
            get; set;
        }

        public virtual DbSet<UserProfile> UserProfiles
        {
            get; set;
        }
        public void DetachAllEntities()
        {
            foreach (var entity in this.ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            {
                this.Entry(entity.Entity).State = EntityState.Detached;
            }
        }

    }
}
