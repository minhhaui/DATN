using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_Ver1.Models
{
    public class _CopyIDsDTO
    {
        public string CopyID { get; set; }
        public string Title { get; set; }
        public string ShelfCode { get; set; }
        public int? ShelfLevel { get; set; }
        public int? ShelfPosition { get; set; }
        public decimal? Price { get; set; }
        public string roomName{ get; set; }
        public string Status { get; set; }  
    }
}