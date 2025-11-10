using Microsoft.EntityFrameworkCore;

namespace ChallengeApp.API.Data
{
    public class ChallengeAppDbContext : DbContext
    {
        public ChallengeAppDbContext(DbContextOptions<ChallengeAppDbContext> options) : base(options) { }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentShare> DocumentShares { get; set; }
        public DbSet<DocumentTag> DocumentTags { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

    }
}