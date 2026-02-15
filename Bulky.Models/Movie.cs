using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bulky.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        public string? Genre { get; set; }

        // This is the special field for your AI Script
        [DisplayName("AI Generated Script")]
        public string? MovieScript { get; set; }
    }
}