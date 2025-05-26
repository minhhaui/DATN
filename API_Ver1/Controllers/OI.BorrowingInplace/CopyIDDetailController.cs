using API_Ver1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API_Ver1.Controllers
{
    public class CopyIDDetailController : ApiController
    {
        HAUI_LIBRARY_25Entities db = new HAUI_LIBRARY_25Entities();
        [HttpGet]
        [Route("api/copyIDDetail/{copyID}")]
        public IHttpActionResult GetCopyIDDetail(string copyID)
        {
            var dkcb = db.BookCopies
                .Where (r => r.CopyID == copyID)
                .Select(r => new _CopyIDsDTO
                {
                    CopyID = r.CopyID,
                    Title = r.Book.Title,
                    Price = r.Book.Price,
                    ShelfCode = r.StorageLocation.ShelfCode,
                    ShelfLevel = r.StorageLocation.ShelfLevel,
                    ShelfPosition = r.StorageLocation.ShelfPosition,
                    roomName = r.StorageLocation.Room.RoomName,
                    Status = r.Status ? "Available" : "Unavailable",
                })
                .ToList();

            return Ok(dkcb);
        }
    }
}
