using System.ComponentModel.DataAnnotations.Schema;

namespace api.cabcheap.com.Models {


    [NotMapped]
    public class ProviderUserDetails{

        public string Name { get; set; }
        public string FirstName { get; set; }        

        public string LastName { get; set; }

        public string Locale { get; set; }

        public string ProviderUserId { get; set; }

        public string Email { get; set; }
    }

}