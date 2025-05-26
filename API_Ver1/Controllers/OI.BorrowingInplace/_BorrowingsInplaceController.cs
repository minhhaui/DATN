using API_Ver1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API_Ver1.Controllers
{
    public class _BorrowingsInplaceController : ApiController
    {
        HAUI_LIBRARY_25Entities db = new HAUI_LIBRARY_25Entities();
        [HttpPost]
        [Route("api/borrowings/inplace")]
        public IHttpActionResult BorrowInPlace([FromBody] _BorrowingsDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.StudentCode) || string.IsNullOrWhiteSpace(request.Email) || request.CopyIDs == null || !request.CopyIDs.Any())
            {
                return BadRequest("Thiếu mã sinh viên, email hoặc danh sách CopyID.");
            }

            db.Database.CommandTimeout = 60;

            var user = db.Users.AsNoTracking().FirstOrDefault(u => u.StudentCode == request.StudentCode);
            if (user == null)
            {
                return NotFound();
            }

            var now = DateTime.Now;
            var dueDate = now.AddDays(1);
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Tạo bản ghi Borrowing
                    var borrowing = new Borrowing
                    {
                        UserID = user.UserID,
                        CreatedAt = now,
                        Email = request.Email,
                        Type = true,
                       // UpdatedAt = now
                    };

                    db.Borrowings.Add(borrowing);
                    db.SaveChanges(); // Để lấy BorrowID

                    var borrowCode = borrowing.BorrowCode;

                    // 2. Duyệt qua từng CopyID để thêm vào BorrowingDetails
                    foreach (var copyId in request.CopyIDs)
                    {
                        var borrowingDetail = new BorrowingDetail
                        {
                            BorrowID = borrowing.BorrowID,
                            CopyID = copyId,
                            BorrowDate = now,
                            DueDate = dueDate,         //Mượn tại chỗ cho hạn là 1 ngày
                            ReturnDate = null,
                            Status = false,           
                        };

                        db.BorrowingDetails.Add(borrowingDetail);
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    return Ok(new
                    {
                        Message = "Đọc/mượn tại chỗ thành công.",
                        BorrowID = borrowing.BorrowID,
                        BorrowCode = borrowCode,
                        TotalBooks = request.CopyIDs.Count
                    });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return InternalServerError(ex);
                }
            }
        }
    }
}
