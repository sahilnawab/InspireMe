using System.ComponentModel.DataAnnotations;

namespace InspireMe.Models
{
    public class QuoteModel
    {
        public int id { get; set; }
        [Required(ErrorMessage = "Author Name is Required")]
        [MinLength(3)]
        public string author { get; set; }

        [Required(ErrorMessage = "Content is Required")]
        [MinLength(3)]
        public string content { get; set; }

        [Required(ErrorMessage ="Imgae is Reqeuired")]
        public IFormFile? Image { get; set; }
        public string? ImagePath { get; set; }
    }
}
