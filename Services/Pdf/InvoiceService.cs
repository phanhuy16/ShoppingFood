using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ShoppingFood.Models;
using ShoppingFood.Repository;

namespace ShoppingFood.Services.Pdf
{
    public class InvoiceService : IInvoiceService
    {
        private readonly DataContext _dataContext;

        public InvoiceService(DataContext dataContext)
        {
            _dataContext = dataContext;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(string orderCode)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(x => x.OrderCode == orderCode);
            if (order == null) return null;

            var details = await _dataContext.OrderDetails
                .Include(x => x.Product)
                .Where(x => x.OrderCode == orderCode)
                .ToListAsync();

            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.UserName == order.UserName);
            var address = user != null
                ? await _dataContext.UserAddresses.FirstOrDefaultAsync(a => a.UserId == user.Id && a.IsDefault)
                : null;

            decimal subtotal = details.Sum(x => x.Price * x.Quantity);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("SHOPPING FOOD").FontSize(24).Bold().FontColor(Colors.Green.Medium);
                            col.Item().Text("Địa chỉ: 123 Đường ăn uống, TP. Hồ Chí Minh");
                            col.Item().Text("Điện thoại: 0123 456 789");
                            col.Item().Text("Email: support@shoppingfood.com");
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("HÓA ĐƠN BÁN HÀNG").FontSize(20).Bold();
                            col.Item().Text($"Mã đơn: #{order.OrderCode}");
                            col.Item().Text($"Ngày: {order.CreatedDate:dd/MM/yyyy HH:mm}");
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Item().PaddingBottom(0.5f, Unit.Centimetre).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("KHÁCH HÀNG:").Bold();
                                col.Item().Text(address?.FullName ?? order.UserName);
                                col.Item().Text(address?.PhoneNumber ?? "");
                                col.Item().Text($"{address?.DetailedAddress}, {address?.Phuong}, {address?.Quan}, {address?.Tinh}");
                            });
                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Text("THANH TOÁN:").Bold();
                                col.Item().Text(order.PaymentMethod);
                                col.Item().Text($"Trạng thái: {(order.Status == 0 ? "Đã hoàn thành" : (order.Status == 1 ? "Mới" : "Đã hủy"))}");
                            });
                        });

                        x.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("#");
                                header.Cell().Element(CellStyle).Text("Sản phẩm");
                                header.Cell().Element(CellStyle).AlignRight().Text("Đơn giá");
                                header.Cell().Element(CellStyle).AlignCenter().Text("SL");
                                header.Cell().Element(CellStyle).AlignRight().Text("Thành tiền");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.Bold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            for (int i = 0; i < details.Count; i++)
                            {
                                var item = details[i];
                                var total = item.Price * item.Quantity;

                                table.Cell().Element(CellStyle).Text((i + 1).ToString());
                                table.Cell().Element(CellStyle).Text(item.Product.Name + (string.IsNullOrEmpty(item.VariantName) ? "" : $" ({item.VariantName})"));
                                table.Cell().Element(CellStyle).AlignRight().Text(item.Price.ToString("#,##0 VND"));
                                table.Cell().Element(CellStyle).AlignCenter().Text(item.Quantity.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text(total.ToString("#,##0 VND"));

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.PaddingVertical(5).BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten2);
                                }
                            }
                        });

                        x.Item().AlignRight().PaddingTop(1, Unit.Centimetre).Column(col =>
                        {
                            col.Item().Text(row =>
                            {
                                row.Span("Tạm tính: ").Bold();
                                row.Span(subtotal.ToString("#,##0 VND"));
                            });
                            col.Item().Text(row =>
                            {
                                row.Span("Phí vận chuyển: ").Bold();
                                row.Span(order.ShippingCode.ToString("#,##0 VND"));
                            });
                            col.Item().Text(row =>
                            {
                                row.Span("Tổng cộng: ").FontSize(14).Bold().FontColor(Colors.Red.Medium);
                                row.Span((subtotal + order.ShippingCode).ToString("#,##0 VND")).FontSize(14).Bold().FontColor(Colors.Red.Medium);
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Cảm ơn quý khách đã mua sắm tại ");
                        x.Span("SHOPPING FOOD").Bold().FontColor(Colors.Green.Medium);
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
