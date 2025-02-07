using ShoppingFood.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingFood.Models
{
    public class ContactModel
    {
        [Key]
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; }
        public string Map { get; set; }
        public string Logo { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile LogoUpload { get; set; } = null!;
    }
}
