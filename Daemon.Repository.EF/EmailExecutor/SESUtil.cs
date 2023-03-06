using System;
using System.Net;
using System.Net.Mail;
using Daemon.Common.Middleware;
using Microsoft.Extensions.DependencyInjection;
namespace Daemon.Repository.EF.Executer.EmailExecutor
{
    public static class SESUtil
    {
        /// <summary>
        /// Send email with AWS SES
        /// </summary>
        /// <param name="emailAddress">email Address</param>
        /// <param name="subject">email subject</param>
        /// <param name="body">email body</param>
        /// <param name="isHtml">email body is html</param>
        /// <returns>send success or error</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1123:DoNotPlaceRegionsWithinElements", Justification = "Code will be change in future")]
        public static bool SendEmail(string emailAddress, string subject, string body, bool isHtml, string clientKey)
        {
            #region SES

            // For aws ses , after get awsAccessKeyId and awsSecretAccessKey ,this will replace old send email function
            // var client = new AmazonSimpleEmailServiceClient(awsAccessKeyId: "", awsSecretAccessKey: "");
            // client.SendEmail(new SendEmailRequest
            // {
            // 	Destination = new Destination
            // 	{
            // 		ToAddresses = new List<string> { emailAddress }
            // 	},
            // 	Message = new Message
            // 	{
            // 		Body = new Body
            // 		{
            // 			Html = new Content
            // 			{
            // 				Charset = "UTF-8",
            // 				Data = body
            // 			}
            // 		},
            // 		Subject = new Content
            // 		{
            // 			Charset = "UTF-8",
            // 			Data = subject
            // 		}
            // 	},
            // 	ReturnPath = "",
            // 	ReturnPathArn = "",
            // 	Source = "sender@example.com",
            // 	SourceArn = ""
            // });
            #endregion

            #region Send email old function
            try
            {
                var clientConfigRepository = ServiceLocator.Resolve<ClientConfigRepository>();
                var clientConfig = clientConfigRepository.Find();
                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.To.Add(emailAddress);
                    mailMessage.Subject = subject;
                    mailMessage.IsBodyHtml = isHtml;
                    mailMessage.From = new MailAddress(clientConfig.EmailAddress, clientConfig.EmailName);
                    mailMessage.Body = body;
                    using (SmtpClient client = new SmtpClient
                    {
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        EnableSsl = clientConfig.SMTPSSL,
                        Host = clientConfig.SMTPHost,
                        Port = clientConfig.SMTPPort.Value,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(clientConfig.SMTPUserName, clientConfig.SMTPPassword),
                        Timeout = 10000,
                    })
                    {
                        client.Send(mailMessage);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

            #endregion
        }
    }
}
