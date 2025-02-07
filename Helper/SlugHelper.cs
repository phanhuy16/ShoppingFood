using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
namespace ShoppingFood.Helper
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // Chuẩn hóa chuỗi (normalize) để xử lý các ký tự có dấu
            string normalizedString = name.Normalize(NormalizationForm.FormD);

            // Loại bỏ các ký tự không phải ASCII
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Loại bỏ các ký tự đặc biệt còn lại và thay khoảng trắng bằng dấu '-'
            string slug = stringBuilder.ToString()
                                       .Normalize(NormalizationForm.FormC) // Normalize lại chuỗi
                                       .ToLowerInvariant(); // Chuyển về chữ thường

            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", ""); // Loại bỏ ký tự không hợp lệ
            slug = Regex.Replace(slug, @"\s+", " ").Trim();  // Xóa khoảng trắng thừa
            slug = slug.Replace(" ", "-");                  // Thay khoảng trắng bằng dấu gạch ngang

            return slug;
        }
    }

}
