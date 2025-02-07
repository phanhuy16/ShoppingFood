using System.ComponentModel.DataAnnotations;

namespace ShoppingFood.Models
{
    public class LoginModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhật username")]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
    public class RegisterModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="Vui lòng nhật username")]
        public string Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage ="Vui lòng nhập email")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage ="Vui lòng nhập mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Vui lòng nhập xác mật khẩu")]
        public string ConfirmPassword { get; set; }
    }
}
