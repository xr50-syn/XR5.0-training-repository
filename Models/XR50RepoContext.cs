using Microsoft.EntityFrameworkCore;
using XR5_0TrainingRepo.Models;

namespace XR5_0TrainingRepo.Models
{
    public class XR50RepoContext : DbContext
    {
        public XR50RepoContext(DbContextOptions<XR50RepoContext> options)
           : base(options)
        {
        }

        public DbSet<XR50App> Apps { get; set; } = null!;
	public DbSet<User> Users { get; set; } = null!;
	public DbSet<TrainingModule> Trainings { get; set; } = null!;
	public DbSet<ResourceBundle> Resources { get; set; } = null!;
	public DbSet<OwncloudShare> OwncloudShares{ get; set; } = null!;
	public DbSet<Asset> Assets { get; set; } = null!;

    }
}
