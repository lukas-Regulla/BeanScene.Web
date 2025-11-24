using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;

namespace BeanScene.Web.Models;

public partial class SittingSchedule
{
    public int SittingScheduleId { get; set; }

    public string Stype { get; set; } = null!;

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public int Scapacity { get; set; }

    public string Status { get; set; }

    //public bool IsClosed { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
