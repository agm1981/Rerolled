using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MySqlDAL
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

        public virtual DbSet<UserName> UserNames
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
