using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.cabcheap.com.Models
{
    public class Waypoint
    {
        [Key]
        public int Id { get; set; }

        public Location Location { get; set; }

        //CREATED AT
        private DateTime _CreatedAt = DateTime.Now;
        [DataType(DataType.Date)]
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "Creation Date")]
        public DateTime CreatedAt { get { return _CreatedAt; } set { _CreatedAt = value; } }

        //UPDATED AT
        private DateTime _UpdatedAt = DateTime.Now;
        [DataType(DataType.Date)]
        [HiddenInput(DisplayValue = true)]
        [Display(Name = "Updated at")]
        public DateTime UpdatedAt { get { return _UpdatedAt; } set { _UpdatedAt = value; } }
    }
}
