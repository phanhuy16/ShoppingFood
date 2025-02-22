using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingFood.Models
{
    public class CouponModel : CommonAbstract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage =("Vui lòng nhập tên coupon"))]
        public string Name { get; set; }

        [Required(ErrorMessage = ("Vui lòng nhập mô tả"))]
        public string Description { get; set; }

        [Required(ErrorMessage = ("Vui lòng nhập số lượng coupon"))]
        public int Quantity { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateExpired { get; set; }
        public int Status { get; set; }
    }
}
