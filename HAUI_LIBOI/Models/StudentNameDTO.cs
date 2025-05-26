using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAUI_LIBOI.Models
{
    internal class StudentNameDTO
    {
        [JsonProperty("studentName")]
        public string StudentName { get; set; }   
    }
}
