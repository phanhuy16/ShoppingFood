using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingFood.Models
{
    public class ProductQuantityModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng sản phẩm")]
        public int Quantity { get; set; }

        [ForeignKey(nameof(ProductId))]
        public int ProductId { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual ProductModel Product { get; set; }
    }
}
