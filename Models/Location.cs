using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.cabcheap.com.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }

        //LATITUDE 11 8
        [RegularExpression(@"^\d{11,8}$")]
        [Range(0, 99999999999.99999999)]
        public double Latitude { get; set; }

        //LONGITUDE 11 8
        [RegularExpression(@"^\d{11,8}$")]
        [Range(0, 99999999999.99999999)]
        public double Longitude { get; set; }

    }
}
