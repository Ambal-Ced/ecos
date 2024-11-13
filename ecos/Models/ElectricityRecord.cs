using System.ComponentModel.DataAnnotations;
using System;
namespace ecos.Models
{
    //all
    public class ElectricityRecord
    {
        [Key]
        public int Id { get; set; } // Primary key

        [Required]
        public string HouseholdName { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Please enter valid positive number")]
        public double ElectricityRate { get; set; }  // For example, rate per kWh

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Please enter valid positive number")]
        public double TotalBill { get; set; }  // Total bill for the month

        [Required]
        [Display(Name = "Billing Month")]
        public DateTime Month { get; set; }
        // New property for User ID
        public string UserId { get; set; } // Forer User

        public DateTime DateCreated { get; set; }
    }
}
