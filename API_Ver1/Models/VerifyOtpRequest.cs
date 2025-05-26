using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_Ver1.Models
{
    public class VerifyOtpRequest
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}