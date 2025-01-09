using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using XR5_0TrainingRepo.Models;

namespace XR5_0TrainingRepo.Models {
    public class ResourceBundle
{
        public string? AppName { get; set; }
        public string? Description { get; set; }
        public string? OwncloudFileName { get; set; }
        public string? TrainingName { get; set; }
        public string? ResourceName { get; set; }
	    [ForeignKey("Assets")]
        public virtual List<string>? AssetList { get; set; }
	    [Key]
        public string? ResourceId {get; set;}

        public ResourceBundle()
        {
	        ResourceId= Guid.NewGuid().ToString();
            AssetList = new List<string>();
        }
    }
}
