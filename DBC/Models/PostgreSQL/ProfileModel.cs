using DBC.Models.Elastic;
using Elastic.Clients.Elasticsearch.Core.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Principal;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DBC.Models.PostgreSQL
{
    public class ProfileModel
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; } 
        public string? Name { get; set; }

        public string? Surname { get; set; }

        public string? Patronymic { get; set; }

        public string  Email { get; set; } = " ";

        public string? Company { get; set; }

        public DateTime ApplyDate { get; set; }
        public DateTime? FireDate { get; set; }
        public string? Appointment { get; set; }

        public string? City { get; set; }
        public string ImgSrc { get; set; } = ".";

        public List<ADAccountModel> ADAccounts { get; set; } = new();

        public bool isIndexed { get; set; } = false;
        public ElasticProfileModel ToElasticProfileModel()
        {
            var profiles = new List<Dictionary<string, object>>();

            if (ADAccounts.Count > 0)
            {


                foreach (var acc in ADAccounts)
                {
                    profiles.Add(new Dictionary<string, object>
                    {
                        ["AD"] = acc.ToElastic()
                    });
                }
            }
            var elasticProfile = new ElasticProfileModel
            {
                Id = Id,
                Created = Created,
                Name = Name ?? "",
                Surname = Surname ?? "",
                Patronymic = Patronymic ?? "",
                Email = Email,
                Company = Company ?? "",
                ApplyDate = ApplyDate,
                FireDate = FireDate,
                Appointment = Appointment ?? "",
                City = City ?? "",
                ImgSrc =    ImgSrc,
                Profiles = profiles
            };
            return elasticProfile;
        }
    }
}
