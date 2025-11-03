using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BeanScene.Web.Models;

public partial class Reservation
{
    public int ReservationId { get; set; }

    [Required, Display(Name = "Sitting")]
    public int SittingId { get; set; }

    [Required, StringLength(50)]
    public string FirstName { get; set; } = null!;

    [Required, StringLength(50)]
    public string LastName { get; set; } = null!;
    
    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [Required, Display(Name = "Start time")]
    public DateTime StartTime { get; set; }

    [Range(15, 300)]
    public int Duration { get; set; }

    [Required, Range(1, 20), Display(Name = "Guests")]
    public int NumOfGuests { get; set; } = 2;

    [Required, Display(Name = "Source")]
    public string ReservationSource { get; set; } = "Online";

    public string? Notes { get; set; }

 
    public string Status { get; set; } = "Pending";

    // You won't post this from the form; set it in the controller
    public DateTime CreatedAt { get; set; }

    // IMPORTANT: navigation properties should not be validated on Create
    [ValidateNever] public virtual SittingSchedule? Sitting { get; set; }  // make nullable
    [ValidateNever] public virtual ICollection<RestaurantTable> RestaurantTables { get; set; } = new List<RestaurantTable>();
}
