using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Foundoc.Autofac;
using Foundoc.Core;
using Foundoc.Manager.Consumer.Logging;
using Foundoc.Manager.Consumer.Models;
using Owin;

namespace Foundoc.Manager.Consumer
{
    public class Startup
    {
        public static string ApplicationName { get; set; }

        public void Configuration(IAppBuilder app)
        {
            var foundationDbStorageOptions = new FoundationDbStorageOptions().GetFromConfigFile();

            if (!string.IsNullOrWhiteSpace(ApplicationName))
            {
                foundationDbStorageOptions.ApplicationName = ApplicationName;
            }

            var builder = new ContainerBuilder();
            builder.RegisterModule(new DefaultFounDocModule
            {
                FoundationDbStorageOptions = foundationDbStorageOptions,
                FoundocSettings = new FoundocSettings()
            });

            var container = builder.Build();

            var documentStore = container.Resolve<IFoundocDocumentStore>();

            InitializeDocumentStore(documentStore).Wait();

            Log4NetLogProvider.ProviderIsAvailableOverride = true;

            app.UseFoundocManager(new FoundocManagerOptions
            {
                DocumentStore = documentStore
            });
        }

        private async Task InitializeDocumentStore(IFoundocDocumentStore documentStore)
        {
            await documentStore.Maintenance.RegisterKeyAsync<Profile>(profile => new { profile.SiteCode, profile.Id});

            await documentStore.Maintenance.RegisterKeyAsync<MasterProfile>(masterProfile => new {masterProfile.Id});

            await documentStore.Maintenance.RegisterIndexAsync(new FoundocIndex<Profile>
            {
                IndexType = IndexType.ImmediatelyConsistent,
                IsUniqueConstraint = true,
                Map = profile => new {profile.SiteCode, profile.Email},
                Name = IndexNames.ByEmail
            }, new CancellationToken());

            await documentStore.Maintenance.RegisterIndexAsync(new FoundocIndex<Profile>
            {
                IndexType = IndexType.ImmediatelyConsistent,
                IsUniqueConstraint = true,
                Map = profile => new { profile.SiteCode,  profile.Username },
                Name = IndexNames.ByUsername
            }, new CancellationToken());


            await documentStore.Initialize();

            for (int i = 1; i <= 20; i++)
            {
                documentStore.OpenTransactionAsync(async transaction =>
                {
                    var profile = new Profile()
                    {
                        Id = i,
                        FirstName = "Test",
                        LastName = "User",
                        Address = "Test Address",
                        DOB = DateTime.UtcNow,
                        Email = "test"+i+"@email.com",
                        Username = "testusername"+i,
                        SiteCode = "bingogodz",
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow,
                    };

                    var retrievedProfile = await transaction.LoadAsync<Profile>(new {profile.SiteCode, profile.Id});

                    if (retrievedProfile == null)
                    {
                        await transaction.StoreAsync(profile);
                    }
                }).GetAwaiter().GetResult();
            }
        }
    }
}
