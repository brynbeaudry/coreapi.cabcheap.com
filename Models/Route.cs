using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.cabcheap.com.Models
{
    public class Route
    {
        [Key]
        public int Id { get; set; }

        //TRIP THAT THE ROUTE IS ON, IF ANY
        public Trip Trip { get; set; }

        //ASSOCIATED USER FOR THIS ROUTE
        public ApplicationUser User { get; set; }

        //START WAYPOINT 
        public Waypoint StartWaypoint { get; set; }


        //END WAYPOINT
        public Waypoint EndWaypoint { get; set; }

        //THE START POINT'S POSITION ON TRIP, IF ANY
        public int StartTripPostion { get; set; }

        //THE START POINT'S POSITION ON TRIP, IF ANY
        public int EndTripPostion { get; set; }


        //START TIME - Chosen by user.
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartTime { get; set; }

        //END TIME - calulated by distance over driving speed. Approximate
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndTime { get; set; }

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

        //COST - calculated from rate over distance and time.
        [RegularExpression(@"^\d+\.\d{0,2}$")]
        [Range(0, 9999999999999.99)]
        [Column(TypeName = "decimal(13,2)")]
        public decimal Cost { get; set; }

    }
}
