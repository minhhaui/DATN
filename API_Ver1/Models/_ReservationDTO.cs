using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace API_Ver1.Models
{
    public class _ReservationDTO
    {
        //public string StudentCode { get; set; }

        public string CopyID { get; set; } // mã đăng kí cá biệt

        public string Title { get; set; }

        public decimal? Price { get; set; }

        public string ShelfCode { get; set; }
        public int? ShelfLevel { get; set; }
        public int? ShelfPosition { get; set; }

        //public DateTime? ReservationDate { get; set; }

        //[JsonConverter(typeof(CustomDateConverter))]
        //public DateTime? ExpireDate { get; set; }
    }
}