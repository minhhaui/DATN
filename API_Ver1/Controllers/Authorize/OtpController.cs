using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Http;
using API_Ver1.Models;
namespace API_Ver1.Controllers
{
    public class OtpController : ApiController
    {
        // Lưu OTP tạm thời theo Email
        private static Dictionary<string, (string Code, DateTime ExpireAt)> otpStore = new Dictionary<string, (string Code, DateTime ExpireAt)>();

        // Gửi OTP
        [HttpPost]
        [Route("api/otp/send")]
        public async Task<IHttpActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Email))
            {
                return BadRequest("Email không hợp lệ.");
            }

            var otp = GenerateOtp();
            var expire = DateTime.UtcNow.AddMinutes(5);
            otpStore[request.Email] = (otp, expire);

            await EmailService.SendAsync(
                request.Email,
                "Mã xác nhận",
              $@"
                <div style='font-family: Segoe UI, Tahoma, sans-serif; background-color: #f4f6f8; padding: 30px;'>
                    <div style='max-width: 520px; margin: auto; background-color: #ffffff; border-radius: 10px; 
                                box-shadow: 0 0 15px rgba(0,0,0,0.1); padding: 30px;'>
                        <h2 style='color: #2c3e50; text-align: center; margin-bottom: 20px;'>👋 Xin chào từ <span style='color: #007bff;'>LIBOI</span></h2>

                        <p style='font-size: 16px; color: #333;'>Chúng tôi đã nhận được yêu cầu xác thực của bạn. Mã OTP của bạn là:</p>

                        <div style='text-align: center; margin: 30px 0;'>
                            <span style='display: inline-block; font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #d32f2f; 
                                         padding: 15px 30px; border: 2px dashed #d32f2f; border-radius: 8px; background-color: #fff8f8;'>
                                {otp}
                            </span>
                        </div>

                        <p style='font-size: 15px; color: #555;'>Mã có hiệu lực trong vòng <strong>5 phút</strong>. Vui lòng không chia sẻ mã này với bất kỳ ai.</p>

                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;' />

                        <p style='font-size: 13px; color: #777;'>Nếu bạn không yêu cầu mã OTP này, hãy bỏ qua email này.</p>
                        <p style='font-size: 13px; color: #aaa;'>Thư được gửi tự động, vui lòng không phản hồi.</p>

                        <div><p style='display:none;'>Token: {Guid.NewGuid()}</p></div>
                    </div>
                </div>",
                isHtml: true);

            return Ok(new { Message = "Đã gửi mã xác nhận." });
        }

        // Xác minh OTP
        [HttpPost]
        [Route("api/otp/verify")]
        public IHttpActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Email) || string.IsNullOrWhiteSpace(request.Otp))
            {
                return BadRequest("Email hoặc mã xác nhận không hợp lệ.");
            }

            if (!otpStore.ContainsKey(request.Email))
            {
                return Content(HttpStatusCode.BadRequest, new { Message = "Không tìm thấy mã xác nhận." });
            }

            var (code, expire) = otpStore[request.Email];

            if (DateTime.UtcNow > expire)
            {
                otpStore.Remove(request.Email);
                return Content(HttpStatusCode.BadRequest, new { Message = "Mã đã hết hạn." });
            }

            if (code != request.Otp)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = "Mã không đúng." });
            }

            otpStore.Remove(request.Email);
            return Ok(new { Message = "Xác minh thành công." });
        }

        // Hàm sinh OTP ngẫu nhiên 6 chữ số
        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        //dùng SmtpClient sẵn có của .NET
        public static class EmailService
        {
            //public static async Task SendAsync(string toEmail, string subject, string body)
            //{
            //    var fromEmail = "minhnnhaui@gmail.com";
            //    var fromPassword = "mlsw oibd isbh wvkx";

            //    using (var smtp = new SmtpClient("smtp.gmail.com"))
            //    {
            //        smtp.Port = 587;
            //        smtp.Credentials = new NetworkCredential(fromEmail, fromPassword);
            //        smtp.EnableSsl = true;

            //        var mail = new MailMessage(fromEmail, toEmail, subject, body);
            //        await smtp.SendMailAsync(mail);
            //    }
            //}
            public static async Task SendAsync(string toEmail, string subject, string body, bool isHtml = false)
            {
                var fromEmail = "minhnnhaui@gmail.com";
                var fromPassword = "mlsw oibd isbh wvkx";

                using (var smtp = new SmtpClient("smtp.gmail.com"))
                {
                    smtp.Port = 587;
                    smtp.Credentials = new NetworkCredential(fromEmail, fromPassword);
                    smtp.EnableSsl = true;

                    var mail = new MailMessage
                    {
                        From = new MailAddress(fromEmail, "OTP LIBOI"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = isHtml
                    };
                    mail.To.Add(toEmail);

                    await smtp.SendMailAsync(mail);
                }
            }
        }
    }
}
