using System;

namespace Foundoc.Manager.Consumer.Models
{
    public class Profile
    {
        public long Id { get; set; }
        public long MasterProfileId { get; set; }
        public string SiteCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime DOB { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public dynamic MetaData { get; set; }
    }
}
