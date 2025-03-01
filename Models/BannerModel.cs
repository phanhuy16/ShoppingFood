using ShoppingFood.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingFood.Models
{
    public class BannerModel : CommonAbstract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string SubTitle { get; set; }

        [Required]
        public string Description { get; set; }

        public string Image { get; set; }

        public int Status { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile ImageUpload { get; set; } = null!;
    }
}
