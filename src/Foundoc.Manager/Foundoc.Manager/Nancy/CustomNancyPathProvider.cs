using System;
using System.IO;
using Nancy;

namespace Foundoc.Manager.Nancy
{
    public class CustomNancyPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            return EnvironmentHelper.IsDevelopment() ?
              Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Foundoc.Manager") :
              AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}