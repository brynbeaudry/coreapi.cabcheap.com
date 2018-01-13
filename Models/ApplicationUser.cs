using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace api.cabcheap.com.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser 
    {

        public override String UserName { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String FullName { get; set; }
        public override String Email { get; set; }
        
        public String ProviderName { get; set; }

        public String ProviderId { get; set; }  
        public String PictureUrl { get; set; }

        [JsonIgnore]
        public override DateTimeOffset? LockoutEnd { get; set; }
        [JsonIgnore]
        public override bool TwoFactorEnabled { get; set; }
        [JsonIgnore]
        public override bool PhoneNumberConfirmed { get; set; }
        [JsonIgnore]
        public override String PhoneNumber { get; set; }
        [JsonIgnore]
        public override String ConcurrencyStamp { get; set; }
        [JsonIgnore]
        public override String SecurityStamp { get; set; }
        [JsonIgnore]
        public override String PasswordHash { get; set; }
        [JsonIgnore]
        public override bool EmailConfirmed { get; set; }
        [JsonIgnore]
        public override String NormalizedEmail { get; set; }

        
        [JsonIgnore]
        public override String NormalizedUserName { get; set; }

        [JsonIgnore]
        public override bool LockoutEnabled { get; set; }
        [JsonIgnore]
        public override int AccessFailedCount { get; set; }

        [JsonIgnore]
        [ScaffoldColumn(false)]
        [NotMapped]
        public bool SerializeJE { get; set; }
        [JsonIgnore]
        [ScaffoldColumn(false)]
        [NotMapped]
        public bool SerializeCE { get; set; }
        [JsonIgnore]
        [ScaffoldColumn(false)]
        [NotMapped]
        public bool SerializeBio { get; set; }

        //[JsonIgnore]
        public String Bio {get; set;}

        /* public List<Event> CreatedEvents { get; set; }

        public List<EventGuest> JoinedEvents { get; set; }

        public bool ShouldSerializeJoinedEvents()
        {
            return SerializeJE;        
        }

        public bool ShouldSerializeCreatedEvents()
        {
            return SerializeCE;
        }
 */
        public bool ShouldSerializeBio()
        {
            return SerializeBio;
        }

    }
}
