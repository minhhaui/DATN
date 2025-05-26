using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Converters;
namespace API_Ver1.Models
{
    public class CustomDateConverter : IsoDateTimeConverter
    {
        public CustomDateConverter()
        {
            DateTimeFormat = "dd/MM/yyyy";
        }
    }
}