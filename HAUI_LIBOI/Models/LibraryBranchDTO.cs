using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAUI_LIBOI.Models
{
    public class LibraryBranchDTO
    {
        public string BranchName { get; set; }
        public List<RoomsDTO> ListRooms { get; set; }
    }
}
