using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.Middleware
{
    public static class CindiClusterExtensions
    {
        public static IApplicationBuilder UseCindiClusterPipeline(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CindiClusterMiddleware>();
        }
    }
}
