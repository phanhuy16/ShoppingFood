#nullable enable
namespace ShoppingFood.Models
{
    public class CartItemModel
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? VariantName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; } = null!;
        public decimal Total
        {
            get { return Quantity * Price; }
        }

        public CartItemModel() { }
        public CartItemModel(ProductModel product, ProductVariantModel? variant = null)
        {
            ProductId = product.Id;
            ProductName = product.Name;

            // Use PriceSale if available, otherwise Price
            decimal basePrice = (product.PriceSale > 0 && product.PriceSale < product.Price)
                                ? product.PriceSale : product.Price;

            if (variant != null)
            {
                VariantId = variant.Id;
                VariantName = $"{variant.Name}: {variant.Value}";
                Price = basePrice + variant.PricePlus;
            }
            else
            {
                Price = basePrice;
            }

            Quantity = 1;
            Image = product.Image;
        }
    }
}
