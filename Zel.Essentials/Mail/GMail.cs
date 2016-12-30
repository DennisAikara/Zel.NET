// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Mail;

namespace Zel.Mail
{
    public class GMail
    {
        private static void SendEmail(string from, string to, string password, string subject, string body)
        {
            var fromAddress = new MailAddress(from, "From Name");
            var toAddress = new MailAddress(to, "To Name");
            var fromPassword = password;

            using (var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 20000,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            })
            {
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    try
                    {
                        smtp.Send(message);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }
    }
}