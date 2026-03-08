using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingFood.Models
{
    public class UserAddressModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tỉnh/thành")]
        public string Tinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn quận/huyện")]
        public string Quan { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phường/xã")]
        public string Phuong { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể")]
        public string DetailedAddress { get; set; }

        public bool IsDefault { get; set; }
    }
}
