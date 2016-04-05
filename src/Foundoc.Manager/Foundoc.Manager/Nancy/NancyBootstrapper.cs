using Autofac;
using Foundoc.Core;
using Foundoc.Core.Logging;
using Foundoc.Manager.Services;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;

namespace Foundoc.Manager.Nancy
{
    public class NancyBootstrapper : AutofacNancyBootstrapper
    {
        private readonly FoundocManagerOptions _options;

        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        public NancyBootstrapper(FoundocManagerOptions options)
        {
            _options = options;
        }

        protected override void RequestStartup(ILifetimeScope requestContainer, IPipelines pipelines, NancyContext context)
        {
            pipelines.OnError.AddItemToEndOfPipeline((z, a) =>
            {
                Logger.Error("Unhandled error on request: " + context.Request.Url + " : " + a.Message, a);
                return ErrorResponse.FromException(a);
            });

            base.RequestStartup(requestContainer, pipelines, context);
        }

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            // No registrations should be performed in here, however you may
            // resolve things that are needed during application startup.
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_options.DocumentStore).As<IFoundocDocumentStore>();
            builder.RegisterType<FoundocManagerService>().As<IFoundocManagerService>();
            builder.Update(container.ComponentRegistry);
        }

        protected override IRootPathProvider RootPathProvider
        {
            get { return new CustomNancyPathProvider(); }
        }
    }
}
