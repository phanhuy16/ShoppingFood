using System.ComponentModel.DataAnnotations;

namespace ShoppingFood.Models.ViewModel
{
    public class ProductRatingViewModel
    {
        public virtual ProductModel Product { get; set; }
        public virtual ICollection<ReviewModel> Reviews { get; set; } = new HashSet<ReviewModel>();
        [Required(ErrorMessage = "Vui lòng nhập ý kiến của bạn")]
        public string Comment { get; set; } // Cho phép null để chỉ đánh giá sao

        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5")]
        [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá")]
        public int Star { get; set; }
    }
}
