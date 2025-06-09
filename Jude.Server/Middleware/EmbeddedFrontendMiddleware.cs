using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

namespace Jude.Server.Middleware
{
    public class EmbeddedFrontendMiddleware(RequestDelegate next, Assembly assembly, string baseResourcePath, string indexPath)
    {
        private readonly RequestDelegate _next = next;
        private readonly Assembly _assembly = assembly;
        private readonly string _indexPath = indexPath;

        public async Task InvokeAsync(HttpContext context)
        {
            var requestPath = context.Request.Path.Value;

            string logicalResourcePath;
            string currentResourcePrefix = "";
            bool isIndexHtmlRequest = false;

            if (string.IsNullOrEmpty(requestPath) || requestPath == "/")
            {
                logicalResourcePath = _indexPath;
                isIndexHtmlRequest = true;
            }
            else if (requestPath.StartsWith("/assets/", StringComparison.OrdinalIgnoreCase))
            {
                logicalResourcePath = requestPath.Substring("/assets/".Length).Replace('/', '.');
                currentResourcePrefix = "assets.assets.";
            }
            else
            {
                logicalResourcePath = requestPath.TrimStart('/').Replace('/', '.');
            }

            string fullResourceName = $"{_assembly.GetName().Name}.{currentResourcePrefix}{logicalResourcePath}";

            using (var stream = _assembly.GetManifestResourceStream(fullResourceName))
            {
                if (stream != null)
                {
                    string contentType = isIndexHtmlRequest ? "text/html" : GetContentType(Path.GetExtension(requestPath));

                    context.Response.ContentType = contentType;
                    await stream.CopyToAsync(context.Response.Body);
                    return;
                }
            }

            if (!Path.HasExtension(requestPath))
            {
                var fallbackResourceName = $"{_assembly.GetName().Name}.{_indexPath}";
                using var fallbackStream = _assembly.GetManifestResourceStream(fallbackResourceName);
                if (fallbackStream != null)
                {
                    context.Response.ContentType = "text/html";
                    await fallbackStream.CopyToAsync(context.Response.Body);
                    return;
                }
            }

            await _next(context);
        }

        private static string GetContentType(string? extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return "application/octet-stream";
            }

            return extension.ToLowerInvariant() switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".woff" => "font/woff",
                ".woff2" => "font/woff2",
                ".ttf" => "font/ttf",
                ".eot" => "application/vnd.ms-fontobject",
                ".ico" => "image/x-icon",
                _ => "application/octet-stream",
            };
        }
    }

    public static class EmbeddedFrontendExtensions
    {
        public static IApplicationBuilder UseEmbeddedFrontend(this IApplicationBuilder builder,
            string baseResourcePath,
            string indexPath = "index.html")
        {
            return builder.UseMiddleware<EmbeddedFrontendMiddleware>(
                Assembly.GetExecutingAssembly(),
                baseResourcePath,
                indexPath
            );
        }
    }
}