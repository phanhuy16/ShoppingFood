#nullable enable
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoppingFood.Models
{
    public class OrderDetailModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string UserName { get; set; } = null!;

        public string OrderCode { get; set; } = null!;

        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public string? VariantName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        [ForeignKey("ProductId")]
        public virtual ProductModel Product { get; set; } = null!;

        [ForeignKey("VariantId")]
        public virtual ProductVariantModel? Variant { get; set; }
    }
}
