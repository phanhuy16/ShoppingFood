namespace ShoppingFood.Services.Pdf
{
    public interface IInvoiceService
    {
        Task<byte[]> GenerateInvoicePdfAsync(string orderCode);
    }
}
