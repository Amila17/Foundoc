using Foundoc.Core.Logging;
using Nancy;

namespace Foundoc.Manager.Modules
{
    public class FoundocManagerModule : NancyModule
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        public FoundocManagerModule()
        {
            Logger.Info("Foundoc Manger loaded");

            Get["/foundocmanager", true] = async (x, ctx) => View["Manager"];
        }
    }
}
