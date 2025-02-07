using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingFood.Models
{
    public class RatingModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required]
        public string Comment { get; set; }

        [Required(ErrorMessage ="Vui lòng nhập tên của bạn")]
        public string Customer { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email của bạn")]
        public string Email { get; set; }

        public string Star { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual ProductModel Product { get; set; }
    }
}
