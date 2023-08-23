using System.ComponentModel.DataAnnotations;

namespace NewsAPI.Entities.Models
{
    public class UpdateInputNewsModel
    {
        public string Title { get; set; }

        public string Content { get; set; }

        [Range(1, 10)]
        public int UpdateCreatorId { get; set; }
    }
}
