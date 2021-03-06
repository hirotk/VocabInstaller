﻿using System.Data.Entity;

namespace VocabInstaller.Models {
    public class ViDbContext : DbContext {
        public ViDbContext()
            : base("ViConnection") {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Card> Cards { get; set; }
    }
}
