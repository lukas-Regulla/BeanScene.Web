using System.Collections.Generic;

namespace BeanScene.Web.Models
{
    public class AssignTablesViewModel
    {
        public int ReservationId { get; set; }

        // Display info
        public string GuestName { get; set; } = string.Empty;
        public int NumOfGuests { get; set; }
        public string SittingName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        // Tables
        public List<RestaurantTable> AvailableTables { get; set; } = new();
        public List<int> SelectedTableIds { get; set; } = new();
    }
}
