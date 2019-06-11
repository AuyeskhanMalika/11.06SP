using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager11._06._19
{
    public class Service
    {
        public async Task<string> SendEmailAsync(string fromAddress, string fromAddressPassword, string toAddress, string theme, string text)
        {
            const int PORT = 587;

            SmtpClient client = new SmtpClient("smtp.mail.ru", PORT)
            {
                Credentials = new NetworkCredential(fromAddress, fromAddressPassword),
                EnableSsl = true
            };

            MailMessage message = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = theme,
                Body = text
            };

            try
            {
                message.To.Add(new MailAddress(toAddress));
                await client.SendMailAsync(message);
            }
            catch (FormatException)
            {
                return "Recipient's email address is incorrect!";
            }
            catch (Exception)
            {
                return "Error by sending the message";
            }

            return "Message sent successfully!";
        }

        public Task<string> MoveCatalog(string fromAddress, string toAddress)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(fromAddress);

            if (directoryInfo.Exists)
            {
                try
                {
                    directoryInfo.MoveTo(toAddress + @"\" + directoryInfo.Name);
                }
                catch (Exception exception)
                {
                    return Task.FromResult("Directory move error" + exception.Message);
                }
            }
            else
            {
                return Task.FromResult("Directory not found!");
            }

            return Task.FromResult("Directory successfully moved");
        }

        public Task<string> DownloadFile(string fromAddress, string toAddress)
        {
            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFile(fromAddress, toAddress);
                }
                catch (Exception)
                {
                    return Task.FromResult("File Download Error");
                }
            }

            return Task.FromResult("File uploaded successfully");
        }
    }
}
