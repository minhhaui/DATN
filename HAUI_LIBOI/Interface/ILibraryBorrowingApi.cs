using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAUI_LIBOI.Interface
{
    public interface ILibraryBorrowingApi
    {
        Task<string?> SubmitBorrowingAsync(string studentCode, string email, List<string> copyIDs);
    }
}
