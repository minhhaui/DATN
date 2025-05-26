using API_Ver1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API_Ver1.Controllers
{
    public class _BorrowingsController : ApiController
    {
        HAUI_LIBRARY_25Entities db = new HAUI_LIBRARY_25Entities();
        [HttpPost]
        [Route("api/borrowings")]
        public IHttpActionResult BorrowBooks([FromBody] _BorrowingsDTO request)
        {
            // Validate đầu vào
            if (request == null || string.IsNullOrWhiteSpace(request.StudentCode) || string.IsNullOrWhiteSpace(request.Email) || request.CopyIDs == null || !request.CopyIDs.Any())
            {
                return BadRequest("Thiếu mã sinh viên, email hoặc danh sách CopyID.");
            }

            // Tìm user theo StudentCode
            // Tăng thời gian timeout để tránh lỗi (nếu cần)
            db.Database.CommandTimeout = 60;

            // Tối ưu hóa truy vấn
            var user = db.Users.AsNoTracking().FirstOrDefault(u => u.StudentCode == request.StudentCode);
            if (user == null)
            {  
                return NotFound();
            }

            var now = DateTime.Now;
            var dueDate = now.AddDays(100); // Giả định hạn trả là 100 ngày

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
                        Type = false,
                       // UpdatedAt = now
                    };

                    db.Borrowings.Add(borrowing);
                    db.SaveChanges(); // Lưu để có BorrowID

                    
                    string borrowCode = borrowing.BorrowCode;
                    

                    // 3. Duyệt qua từng CopyID để:
                    foreach (var copyId in request.CopyIDs)
                    {
                        // a. // a. Xoá Reservation (nếu có)
                        var reservation = db.Reservations.FirstOrDefault(r =>
                            r.UserID == user.UserID &&
                            r.CopyID == copyId &&
                            r.Status == true); 

                        if (reservation != null)
                        {
                            db.Reservations.Remove(reservation);
                        }

                        // b. Thêm bản ghi vào BorrowingDetails
                        var borrowingDetail = new BorrowingDetail
                        {
                            BorrowID = borrowing.BorrowID,
                            CopyID = copyId,
                            BorrowDate = now,
                            DueDate = dueDate,
                            ReturnDate = null,
                            Status = false,  //đang mượn
                        };

                        db.BorrowingDetails.Add(borrowingDetail);
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    return Ok(new
                    {
                        Message = "Mượn sách thành công.",
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
