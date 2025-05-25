using System.ComponentModel.DataAnnotations;

namespace InspireMe.Models
{
    public class QuoteModel
    {

        [Required(ErrorMessage = "Author Name is Required")]
        [MinLength(3)]
        public string author { get; set; }

        [Required(ErrorMessage = "Content is Required")]
        [MinLength(3)]
        public string content { get; set; }

    }
}
