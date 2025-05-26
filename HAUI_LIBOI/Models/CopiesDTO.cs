using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAUI_LIBOI.Models
{
    public class CopiesDTO
    {
        public string CopyID { get; set; }
        public string Title { get; set; }
        public string RoomName { get; set; }
        public string ShelfCode { get; set; }
        public int? ShelfLevel { get; set; }
        public int? ShelfPosition { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }

        public string Email { get; set; }

        public string BorrowCode { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime BorrowDate { get; set; }
        public bool IsSelected { get; set; } // dùng cho checkbox
    }
}
