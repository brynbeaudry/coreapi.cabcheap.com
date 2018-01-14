using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.cabcheap.com.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }

        //START TIME - Calculated from routes that belong to trip
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartTime { get; set; }

        //END TIME - Calculated from routes that belong to trip
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

        //LIST OF ROUTES
        public List<Route> Routes { get; set; }

        //Cost - Calculated from the cost over distance travels for all routes on trip
        [RegularExpression(@"^\d+\.\d{0,2}$")]
        [Range(0, 9999999999999.99)]
        [Column(TypeName = "decimal(13,2)")]
        public decimal Cost { get; set; }
    }
}
