using System.ComponentModel.DataAnnotations;

namespace ShoppingFood.Models.ViewModel
{
    public class ProductRatingViewModel
    {
        public virtual ProductModel Product { get; set; }
        public virtual ICollection<ReviewModel> Reviews { get; set; } = new HashSet<ReviewModel>();

        [Required(ErrorMessage = "Vui lòng nhập ý kiến của bạn")]
        public string Comment { get; set; }
        public int Star { get; set; }
    }
}
