using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoppingFood.Models
{
    public class WishlistModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual ProductModel Product { get; set; }
    }
}
