using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BeanScene.Web.Models;

public partial class RestaurantTable
{
    public int RestaurantTableId { get; set; }

    [Required]
    [Display(Name = "Area")]
    public int AreaId { get; set; }

    [Required]
    [StringLength(50)]
    public string TableName { get; set; } = string.Empty;

    [Range(1, 100)]                     // adjust max as you like
    public int? Seats { get; set; }

    [ValidateNever]                      // <-- prevent MVC from validating this on Create
    public virtual Area? Area { get; set; }  // <-- make nullable for NRT

    [ValidateNever]
    public virtual ICollection<Reservation> ReservationTables { get; set; } = new List<Reservation>();
}

