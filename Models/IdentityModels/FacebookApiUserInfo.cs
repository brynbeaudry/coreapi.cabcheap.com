using System.ComponentModel.DataAnnotations.Schema;

namespace api.cabcheap.com.Models
{
[NotMapped]
public class FacebookApiUserInfo
{

public string id { get; set; }


public string email { get; set; }


public string name { get; set; }


public string first_name { get; set; }

public string last_name { get; set; }


public string locale { get; set; }


}
}