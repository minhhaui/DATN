using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_Ver1.Models
{
    public class _RoomInforDTO
    {
        public string RoomID { get; set; }
        public string RoomName { get; set; }
        public string RoomType { get; set; } // hoặc string nếu bạn muốn hiển thị "Reading"/"Borrowing"
    }
}