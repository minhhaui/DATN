using API_Ver1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API_Ver1.Controllers
{
    public class BorrowerNameController : ApiController
    {
        HAUI_LIBRARY_25Entities db = new HAUI_LIBRARY_25Entities();
        [HttpGet]
        [Route("api/students/{studentCode}")]
        public IHttpActionResult GetUsers(string studentCode)
        {
            var branches = db.Users
                .Where(b => b.StudentCode == studentCode)
                .Select(b => new _StudentDetailDTO
                {

                    StudentName = b.FullName
                })
                .ToList();

            return Ok(branches);
        }
    }
}
