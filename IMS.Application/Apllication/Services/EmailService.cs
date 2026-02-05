using IMS.APPLICATION.Interface.Services;
using IMS.COMMON.Dtos;
using IMS.Models.Models;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Gmail.v1.Data;

namespace IMS.APPLICATION.Apllication.Services
{
    public class EmailService : IEmailService
    {
        private readonly GmailTokenService _gmailTokenService;

        private readonly string _adminEmail = "sushilchaulagain.079@kathford.edu.np"; // change admin mail here
        private readonly string _fromEmail = "sushilchaulagain25@gmail.com";

        public EmailService(GmailTokenService gmailTokenService)
        {
            _gmailTokenService = gmailTokenService;
        }

        public async Task SendLowStockAlertAsync(List<LowStockAlertDto> alerts)
        {
            if (alerts == null || alerts.Count == 0)
                return;

            var gmailService = await _gmailTokenService.GetGmailServiceAsync();

            string subject = "🚨 IMS LOW STOCK ALERT";
            string bodyHtml = BuildHtmlBody(alerts);

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("IMS Inventory System", _fromEmail));
            email.To.Add(new MailboxAddress("Admin", _adminEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = bodyHtml
            };

            email.Body = builder.ToMessageBody();

            var rawMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(email.ToString()))
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            var gmailMessage = new Message
            {
                Raw = rawMessage
            };

            await gmailService.Users.Messages.Send(gmailMessage, "me").ExecuteAsync();
        }

        private string BuildHtmlBody(List<LowStockAlertDto> alerts)
        {
            var sb = new StringBuilder();

            sb.Append("""
        <h2 style='color:red;'>🚨 Low Stock Alert (IMS)</h2>
        <p>The following products have reached Reorder Point (ROP):</p>

        <table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%;'>
            <tr style='background-color:#f2f2f2;'>
                <th>Product</th>
                <th>Current Stock</th>
                <th>ROP</th>
                <th>Avg Sales/Day</th>
                <th>Lead Time</th>
                <th>Safety Stock</th>
                <th>Urgency</th>
            </tr>
        """);

            foreach (var p in alerts)
            {
                sb.Append($"""
            <tr>
                <td>{p.ProductName}</td>
                <td>{p.CurrentStock}</td>
                <td>{p.ReorderPoint}</td>
                <td>{p.AverageDailySales}</td>
                <td>{p.LeadTimeDays}</td>
                <td>{p.SafetyStock}</td>
                <td><b>{p.UrgencyLevel}</b></td>
            </tr>
            """);
            }

            sb.Append("""
        </table>

        <br/>
        <p style='color:gray;'>Auto-generated from IMS Inventory System.</p>
        """);

            return sb.ToString();
        }
    }
}
