﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ShoppingFood.Repository.Validation;

namespace ShoppingFood.Models
{
    public class ProductModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập tên sản phẩm")]
        public string Name { get; set; } = null!;

        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug chỉ được chứa chữ thường, số và dấu gạch ngang")]
        public string Slug { get; set; } = null!;

        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập mô tả sản phẩm")]
        public string Description { get; set; } = null!;

        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập mô tả sản phẩm")]
        public string Detail { get; set; } = null!;

        public string Image { get; set; } = null!;

        public int Status { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập giá sản phẩm")]
        [Range(0.01, double.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Yêu cầu chọn danh mục")]
        public int CategoryId { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Yêu cầu chọn thương hiệu")]
        public int BrandId { get; set; }

        public int Quantity { get; set; }
        public int Sold { get; set; }

        public virtual CategoryModel Category { get; set; } = null!;
        public virtual BrandModel Brand { get; set; } = null!;
        public virtual RatingModel Rating { get; set; } = null!;

        [NotMapped]
        [FileExtension]
        public IFormFile ImageUpload { get; set; } = null!;
    }
}
