using HAUI_LIBOI.Models;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAUI_LIBOI.Interface
{
    public interface ILibraryCopyIDsinBorrrowingApi
    {
        Task<List<CopiesDTO>?> getCopyIDsinBorrrowing(string studentCode, string roomID);
    }
}
