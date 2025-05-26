using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_Ver1.Models
{
    public class _LibraryBranchInforDTO
    {
        public string BranchName { get; set; }
        public List<_RoomInforDTO> ListRooms { get; set; }
    }
}