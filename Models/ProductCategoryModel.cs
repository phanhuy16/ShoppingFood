using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingFood.Models
{
    public class ProductCategoryModel : CommonAbstract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Slug { get; set; }
        public int Status { get; set; }

        public virtual ICollection<ProductModel> Products { get; set; } = new HashSet<ProductModel>();
    }
}
