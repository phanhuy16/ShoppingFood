#nullable enable
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ShoppingFood.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingFood.Models
{
    public class ProductModel : CommonAbstract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập tên sản phẩm")]
        public string Name { get; set; } = null!;

        [ValidateNever]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug chỉ được chứa chữ thường, số và dấu gạch ngang")]
        public string Slug { get; set; } = null!;

        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập mô tả sản phẩm")]
        public string Description { get; set; } = null!;

        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập mô tả sản phẩm")]
        public string Detail { get; set; } = null!;

        [ValidateNever]
        public string Image { get; set; } = null!;

        public int Status { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập giá sản phẩm")]
        [Range(0.01, double.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập giá khuyến mãi sản phẩm")]
        [Range(0.01, double.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PriceSale { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập giá vốn sản phẩm")]
        [Range(0.01, double.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CapitalPrice { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Yêu cầu chọn danh mục")]
        public int CategoryId { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Yêu cầu chọn thương hiệu")]
        public int BrandId { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Yêu cầu chọn danh mục sản phẩm")]
        [ForeignKey(nameof(ProductCategory))]
        public int ProductCategoryId { get; set; }

        public int Quantity { get; set; }
        public int Sold { get; set; }

        [ValidateNever]
        public virtual CategoryModel Category { get; set; } = null!;
        [ValidateNever]
        public virtual BrandModel Brand { get; set; } = null!;
        [ValidateNever]
        public virtual ICollection<ReviewModel> Reviews { get; set; } = new List<ReviewModel>();
        [ValidateNever]
        public virtual ProductCategoryModel ProductCategory { get; set; } = null!;

        public virtual ICollection<ProductImageModel> ProductImages { get; set; } = new List<ProductImageModel>();
        public virtual ICollection<ProductVariantModel> ProductVariants { get; set; } = new List<ProductVariantModel>();

        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }

        [NotMapped]
        public List<IFormFile>? ImageGalleryUpload { get; set; }

        [NotMapped]
        public string? VariantJson { get; set; }
    }
}
