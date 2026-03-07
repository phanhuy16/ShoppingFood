using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingFood.Models
{
    public class ProductVariantModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên biến thể (ví dụ: Size, Topping)")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập giá trị biến thể (ví dụ: L, M, Phô mai)")]
        public string Value { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập giá cộng thêm")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PricePlus { get; set; } = 0;

        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public virtual ProductModel Product { get; set; } = null!;
    }
}
