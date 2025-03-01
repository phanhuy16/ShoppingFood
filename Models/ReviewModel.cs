using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingFood.Models
{   
    public class ReviewModel : CommonAbstract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [StringLength(450)]
        public string UserId { get; set; }

        [Required]
        public string Comment { get; set; }

        public int Star { get; set; }

        public virtual ICollection<ProductModel> Products { get; set; } = new HashSet<ProductModel>();
        public virtual AppUserModel Users { get; set; }
    }
}
