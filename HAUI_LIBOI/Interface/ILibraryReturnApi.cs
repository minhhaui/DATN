using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAUI_LIBOI.Interface
{
    public interface ILibraryReturnApi
    {
        Task<bool> SubmitReturnAsync(string studentCode, string updatedEmail, List<string> copyIDs);
    }
}
