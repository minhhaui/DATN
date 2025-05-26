using HAUI_LIBOI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAUI_LIBOI.Interface
{
    public interface ILibraryCopyIDDetailApi
    {
        Task<CopiesDTO?> GetCopyIDDetail(string copyID);
    }
}
