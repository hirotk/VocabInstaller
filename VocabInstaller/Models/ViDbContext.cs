using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace VocabInstaller.Models {
    public class ViDbContext : DbContext {
        public ViDbContext()
            : base("DefaultConnection") {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Question> Questions { get; set; }
    }
}
