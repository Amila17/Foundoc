using Foundoc.Core.Logging;
using Foundoc.Manager.Nancy;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace Foundoc.Manager
{
    public static class AppBuilderExtension
    {
        public static IAppBuilder UseFoundocManager(this IAppBuilder app, FoundocManagerOptions options)
        {
            app.UseFileServer(new FileServerOptions
            {
                FileSystem = new PhysicalFileSystem(EnvironmentHelper.IsDevelopment() ? "../../../Foundoc.Manager/Assets" : "Assets"),
                RequestPath = new PathString("/Assets")
            });

            app.UseNancy(opt => opt.Bootstrapper = new NancyBootstrapper(options));

            return app;
        }
    }
}
