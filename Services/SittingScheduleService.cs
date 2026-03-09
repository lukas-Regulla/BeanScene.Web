using System;
using BeanScene.Web.Models;
using Microsoft.EntityFrameworkCore;
using BeanScene.Web.Data;
using Microsoft.Identity.Client;

namespace BeanScene.Web.Services
{
    public class SittingScheduleService
    {
        private readonly BeanSceneContext _context;
        public SittingScheduleService(BeanSceneContext context)
        {
            _context = context;
        }
       
        public async Task EnsureDailySittingsExistAsync(DateTime date)
        {
            date = date.Date; // Normalize to midnight'

            bool exists = await  _context.SittingSchedules
                .AnyAsync(s => s.StartDateTime.Date == date);

            if (exists) return;

            var newSittings = new List<SittingSchedule>
            {
                new SittingSchedule
                {
                    Stype = "Breakfast",
                    StartDateTime = date.AddHours(8), // 8:00 AM
                    EndDateTime = date.AddHours(11), // 11:00 AM
                    Scapacity = 20,
                    Status = SittingStatus.Open
                },
                new SittingSchedule
                {
                    Stype = "Lunch",
                    StartDateTime = date.AddHours(12), // 12:00 PM
                    EndDateTime = date.AddHours(15), // 3:00 PM
                    Scapacity = 30,
                    Status = SittingStatus.Open
                },
                new SittingSchedule
                {
                    Stype = "Dinner",
                    StartDateTime = date.AddHours(18), // 6:00 PM
                    EndDateTime = date.AddHours(22), // 10:00 PM
                    Scapacity = 40,
                    Status = SittingStatus.Open
                }
            };
            _context.SittingSchedules.AddRange(newSittings);
            await _context.SaveChangesAsync();
        }

        public async Task<SittingSchedule> GetOrCreateSittingAsync(DateTime date, string stype)
        {
            date = date.Date;

            await EnsureDailySittingsExistAsync(date);

            var sitting = await _context.SittingSchedules
                .FirstOrDefaultAsync(s =>
                    s.StartDateTime.Date == date &&
                    s.Stype == stype);

            if (sitting == null)
                throw new InvalidOperationException($"No sitting found for {stype} on {date:d}");

            return sitting;
        }

    }

}
