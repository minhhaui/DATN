using API_Ver1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API_Ver1.Controllers
{
    public class _ReturnController : ApiController
    {
        HAUI_LIBRARY_25Entities db = new HAUI_LIBRARY_25Entities();
        [HttpPatch]
        [Route("api/return")]
        public IHttpActionResult ReturnBooks([FromBody] _ReturnDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.StudentCode) || string.IsNullOrEmpty(request.UpdatedEmail) || request.CopyIDs == null || !request.CopyIDs.Any())
            {
                return BadRequest("Invalid request");
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

            // Tìm tất cả BorrowingDetails cần cập nhật
            var borrowingDetails = db.BorrowingDetails
                .Where(d =>
                    request.CopyIDs.Contains(d.CopyID) &&
                    d.Borrowing.UserID == user.UserID &&
                    d.Status == false 
                )
                .ToList();

            if (!borrowingDetails.Any())
            {
                return BadRequest("No matching borrowing details found");
            }

            foreach (var detail in borrowingDetails)
            {
                detail.Status = true; // Đã trả
                detail.ReturnDate = now;

                // Cập nhật bảng Borrowings
                var borrowing = detail.Borrowing;
                borrowing.UpdatedAt = now;
                borrowing.UpdatedEmail = request.UpdatedEmail;
            }

            db.SaveChanges();

            return Ok(new
            {
                Message = "Trả sách thành công",
                TotalReturned = borrowingDetails.Count,
            });
        }

    }
}
