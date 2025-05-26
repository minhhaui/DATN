using API_Ver1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API_Ver1.Controllers
{
    public class RoomsinLibraryBranchController : ApiController
    {
        HAUI_LIBRARY_25Entities db = new HAUI_LIBRARY_25Entities();
        [HttpGet]
        [Route("api/getRoomsbyLibraryBranch")]
        public IHttpActionResult GetRoomsbyLibraryBranch()
        {
            var a = db.LibraryBranches
                .Select(r => new _LibraryBranchInforDTO
                {
                    BranchName = r.BranchName,
                    ListRooms = r.Rooms.Select(v => new _RoomInforDTO
                    {
                        RoomID = v.RoomID,
                        RoomName = v.RoomName,
                        RoomType = v.RoomType ? "Reading" : "Borrowing"
                    }).ToList()
                }).ToList();

            return Ok(a);
        }
    }
}
