using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using XR5_0TrainingRepo.Models;
namespace XR5_0TrainingRepo.Models
{
    public class XR50App
    { 
        [Key]
        public string AppName { get; set; }
	[ForeignKey("Trainings")]
        public virtual List<string>? TrainingList { get; set; }

        public string? OwncloudGroup { get; set; }
        public string? Description { get; set; }
        public string? OwncloudDirectory {  get; set; }
        public string? OwnerName { get; set;} 
	public User? Owner {get; set;}
        public virtual List<string>? AdminList { get; set; }
        
        public XR50App(){ }

    }
}
