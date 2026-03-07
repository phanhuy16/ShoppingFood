using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace ShoppingFood.Models
{   
    public class ReviewModel : CommonAbstract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [StringLength(450)]
        public string UserId { get; set; } = null!;

        [Required]
        public string Comment { get; set; } = null!;

        public int Star { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual ProductModel Product { get; set; } = null!;
        public virtual AppUserModel User { get; set; } = null!;
    }
}
