namespace ShoppingFood.Models.Configuration
{
  public class VnpaySettings
  {
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string CurrCode { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Locale { get; set; } = string.Empty;
    public string PaymentBackReturnUrl { get; set; } = string.Empty;
  }
}
