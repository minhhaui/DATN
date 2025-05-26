using API_Ver1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API_Ver1.Controllers
{
    public class _ReservationsController : ApiController
    {
        HAUI_LIBRARY_25Entities db = new HAUI_LIBRARY_25Entities();
        [HttpGet]
        [Route("api/reservations/{studentCode}/{roomId}")]
        public IHttpActionResult GetReservationsByStudentAndRoom(string studentCode, string roomId)
        {
            var reservations = db.Reservations
                .Where(r =>
                    r.User.StudentCode == studentCode &&
                    r.Status == true &&
                    r.BookCopy.StorageLocation.RoomID == roomId
                )
                .Select(r => new _ReservationDTO
                {
                    CopyID = r.CopyID,
                    Title = r.BookCopy.Book.Title,
                    Price = r.BookCopy.Book.Price,
                    ShelfCode = r.BookCopy.StorageLocation.ShelfCode,
                    ShelfLevel = r.BookCopy.StorageLocation.ShelfLevel,
                    ShelfPosition = r.BookCopy.StorageLocation.ShelfPosition,
                })
                .ToList();

            return Ok(reservations);
        }
    }
}
