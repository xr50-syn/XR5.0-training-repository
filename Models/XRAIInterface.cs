using System.ComponentModel.DataAnnotations;

namespace XR5_0TrainingRepo.Models
{
    public class QueryStore
    {

        public string? Query { get; set; }
        public string? QueryResponse { get; set; }
        [Key]
        public long QueryId { get; set; }

    }
}
