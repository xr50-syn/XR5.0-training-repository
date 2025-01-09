using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using XR5_0TrainingRepo.Models;

namespace XR5_0TrainingRepo.Models
{
    public class TrainingModule
    {

        
        public string? UseCase { get; set; }
	    [Key]
        public string TrainingId { get; set; }
        [ForeignKey("Resources")]
        public virtual List<string>? ResourceList { get; set; }
        [ForeignKey("Assets")]
        public virtual List<string>? AssetList { get; set; }
        public string AppName { get; set; }
        public string TrainingName { get; set; }
         
        public TrainingModule()
        {
            TrainingId= Guid.NewGuid().ToString();;	    
            ResourceList = new List<string>();
            AssetList = new List<string>();

        }
    }

}
