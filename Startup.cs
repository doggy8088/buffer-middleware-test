using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace buffer_middleware
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

        }
        private Stream _original;
        private MemoryStream _buffered;

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Use(async(context, next) =>
            {
                if (context.Response.HasStarted)
                {
                    throw new Exception("Cannot buffer response, it has already started");
                }

                _original = context.Response.Body;
                _buffered = new MemoryStream();

                context.Response.Body = _buffered;

                await next();

                if (_buffered != null)
                {
                    _buffered.Seek(0, SeekOrigin.Begin);
                    context.Response.Body = _original;

                    using(var s = new StreamReader(_buffered))
                    using(var w = new StreamWriter(context.Response.Body))
                    {
                        var str = s.ReadToEnd().Replace("World", "Will");
                        await w.WriteLineAsync(str);
                    }

                    _buffered.Dispose();
                    _buffered = null;
                }
            });

            app.Run(async(context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}