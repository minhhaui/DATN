using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_Ver1.Models
{
    public class _ReturnDTO
    {
        public string StudentCode { get; set; }

        public string UpdatedEmail { get; set; }
        public List<string> CopyIDs { get; set; }
    }
}