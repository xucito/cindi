using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.Authentication
{
    public class BasicAuthenticationFilter: IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            var securityRequirements = new Dictionary<string, IEnumerable<string>>()
        {
            { "Basic", new string[] { } }
        };

            swaggerDoc.Security = new[] { securityRequirements };
        }
    }
}
