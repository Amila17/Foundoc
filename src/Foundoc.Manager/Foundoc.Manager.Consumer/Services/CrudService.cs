using System.Threading.Tasks;
using Foundoc.Core;
using Foundoc.Manager.Consumer.Models;

namespace Foundoc.Manager.Consumer.Services
{
    public class CrudService : ICrudService
    {
        private readonly IFoundocDocumentStore _documentStore;

        public CrudService(IFoundocDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public async Task Create(Profile profile)
        {
            await _documentStore.OpenTransactionAsync(async transaction =>
            {
                await transaction.StoreAsync(profile);
            });
        }

        public async Task Update(Profile profile)
        {
            await _documentStore.OpenTransactionAsync(async transaction =>
            {
                await transaction.StoreAsync(profile);
            });
        }

        public async Task Delete(string siteCode, long id)
        {
            await _documentStore.OpenTransactionAsync(async transaction =>
            {
                await transaction.DeleteAsync<Profile>(new {siteCode, id});

            });
        }

        public async Task<Profile> GetById(string siteCode, long id)
        {
            var profile = default(Profile);

            await _documentStore.OpenTransactionAsync(async transaction =>
            {
                profile = await transaction.LoadAsync<Profile>(new {siteCode, id});

            });

            return profile;
        }

        public async Task<Profile> GetByEmail(string siteCode, string email)
        {
            var profile = default(Profile);

            await _documentStore.OpenTransactionAsync(async transaction =>
            {
                profile = await transaction.GetByIndex<Profile>(IndexNames.ByEmail, new{siteCode, email});

            });

            return profile;
        }

        public async Task<Profile> GetByUsername(string siteCode, string username)
        {
            var profile = default(Profile);

            await _documentStore.OpenTransactionAsync(async transaction =>
            {
                profile = await transaction.GetByIndex<Profile>(IndexNames.ByUsername, new { siteCode, username });

            });

            return profile;
        }
    }
}
