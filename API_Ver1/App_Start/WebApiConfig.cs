using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace API_Ver1
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            // Thêm ApiKeyHandler vào pipeline
            config.MessageHandlers.Add(new ApiKeyHandler());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // ✅ Bỏ XML formatter
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // ✅ Cấu hình JSON formatter
            var jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}
