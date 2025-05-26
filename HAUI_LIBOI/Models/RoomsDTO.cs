using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAUI_LIBOI.Models
{
    public class RoomsDTO
    {
        public string RoomID { get; set; }
        public string RoomName { get; set; }
        public string RoomType { get; set; } // hoặc string nếu bạn muốn hiển thị "Reading"/"Borrowing"
    }
}
