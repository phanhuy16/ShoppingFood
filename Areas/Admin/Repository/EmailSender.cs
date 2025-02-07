using System.Net;
using System.Net.Mail;

namespace ShoppingFood.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            // Logic to send email
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("duythai7140@gmail.com", "vrsyuzzemtqhrgrp")
            };

            return client.SendMailAsync(new MailMessage(from: "duythai7140@gmail.com", to: email, subject, message));
        }
    }
}
