using API_Ver1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API_Ver1.Controllers
{
    public class _AllCopyIDsinBorrowingbyRoomandStudentController : ApiController
    {
        HAUI_LIBRARY_25Entities db = new HAUI_LIBRARY_25Entities();
        [HttpGet]
        [Route("api/copiesinBorrowingbyRoom/{studentCode}/{roomId}")]
        public IHttpActionResult GetCopiesinBorrowingbyRoom(string studentCode, string roomId)
        {
            var borrowedBooks = db.BorrowingDetails
                .Where(bd =>
                    bd.Borrowing.User.StudentCode == studentCode &&
                    bd.Borrowing.User != null &&
                    bd.ReturnDate == null &&
                    bd.Status == false &&
                    bd.BookCopy.StorageLocation.RoomID == roomId 
                )
                .Select(bd => new _CopiesinBorrrowingbyRoom
                {
                    CopyID = bd.CopyID,
                    Title = bd.BookCopy.Book.Title,
                    ShelfCode = bd.BookCopy.StorageLocation.ShelfCode,
                    ShelfLevel = bd.BookCopy.StorageLocation.ShelfLevel,
                    ShelfPosition = bd.BookCopy.StorageLocation.ShelfPosition,
                    Price = bd.BookCopy.Book.Price,
                    BorrowCode = bd.Borrowing.BorrowCode,
                    Email = bd.Borrowing.Email,
                    BorrowDate = bd.BorrowDate,
                    DueDate = bd.DueDate,
                })
                .ToList();

            return Ok(borrowedBooks);
        }
    }
}
