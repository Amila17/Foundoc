using System.Threading.Tasks;
using Foundoc.Manager.Consumer.Models;

namespace Foundoc.Manager.Consumer.Services
{
    public interface ICrudService
    {
        Task Create(Profile profile);
        Task Update(Profile profile);
        Task Delete(string siteCode, long id);
        Task<Profile> GetById(string siteCode, long id);
        Task<Profile> GetByEmail(string siteCode, string email);
        Task<Profile> GetByUsername(string siteCode, string username);
    }
}