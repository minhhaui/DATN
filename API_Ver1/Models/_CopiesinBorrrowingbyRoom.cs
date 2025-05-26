using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_Ver1.Models
{
    public class _CopiesinBorrrowingbyRoom
    {
        public string CopyID { get; set; }
        public string Title { get; set; }
        public string ShelfCode { get; set; }
        public int? ShelfLevel { get; set; }
        public int? ShelfPosition { get; set; }
        public decimal? Price { get; set; }
        public string BorrowCode { get; set; }
        public DateTime? BorrowDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Email { get; set; }
    }
}