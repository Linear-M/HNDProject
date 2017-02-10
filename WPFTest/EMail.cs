using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;


namespace WPFTest
{
    class EMail
    {
        public string sendit(string MessageBody)
        {
            MailMessage msg = new MailMessage();

            msg.From = new MailAddress("bapople@gmail.com");
            msg.To.Add("benpople@outlook.com");
            msg.Subject = "Better Project Weekly Notification";
            msg.Body = MessageBody;
            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("bapople@gmail.com", "ss"); 
            try
            {
                client.Send(msg);
                //return "Sent email to " + LoginHandler.email.ToString();
                return "EMail sent successfully";
            }
            catch (Exception ex)
            {
                return "Could not send email, stack: " + ex.Message;
            }
            finally
            {
                msg.Dispose();
            }
        }

        public void sendEmail(string body)
        {
            try
            {
                string error = sendit(body);
                Trace.WriteLine(error);
            }
            catch (Exception)
            {
                Trace.WriteLine("Email error - likely with credentials");   
            }
        }
    }
}
