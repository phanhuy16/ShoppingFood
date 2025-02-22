using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoppingFood.Models
{
    public class BrandModel : CommonAbstract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập tên thương hiệu")]
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập mô tả thương hiệu")]
        public string Description { get; set; } = null!;
        public string Image { get; set; } = null!;
        public int Status { get; set; }
    }
}
