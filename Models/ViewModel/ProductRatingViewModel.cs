using System.ComponentModel.DataAnnotations;

namespace ShoppingFood.Models.ViewModel
{
    public class ProductRatingViewModel
    {
        public virtual ProductModel Product { get; set; }


        [Required(ErrorMessage = "Vui lòng nhập ý kiến của bạn")]
        public string Comment { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên của bạn")]
        public string Customer { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email của bạn")]
        public string Email { get; set; }
    }
}
