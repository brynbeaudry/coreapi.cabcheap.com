using System.ComponentModel.DataAnnotations.Schema;

namespace api.cabcheap.com.Models
{
    [NotMapped]
    public class FacebookApiTokenInfo
    {

    public string app_id { get; set; }


    public string type { get; set; }


    public string application { get; set; }


    public string expires_at { get; set; }

    public string is_valid { get; set; }


    public string issued_at { get; set; }

    public string[] scopes { get; set; }


    public string user_id { get; set; }

    }
}