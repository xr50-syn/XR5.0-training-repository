using System.ComponentModel.DataAnnotations;
using XR5_0TrainingRepo.Models;

namespace XR5_0TrainingRepo.Models
{

    public class OwncloudShare
    {

        public string? Path { get; set; }
        public string? Description { get; set; }
        public string? OwncloudFileName { get; set; }
        public string? TrainingName { get; set; }
        public string? ResourceName { get; set; }
        public string? AppName { get; set; }
        public string? AssetId { get; set; }
        public string? Type { get; set; }
        public string Target { get; set; }

        [Key]
        public string ShareId { get; set; }
        public OwncloudShare()
        {
            ShareId= Guid.NewGuid().ToString();
        }
    }
}
