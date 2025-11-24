using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeanScene.Web.Data;
using BeanScene.Web.Models;
using Microsoft.AspNetCore.Authorization;

namespace BeanScene.Web.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class ReservationsController : Controller
    {
        private readonly BeanSceneContext _context;

        public ReservationsController(BeanSceneContext context)
        {
            _context = context;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var beanSceneContext = _context.Reservations.Include(r => r.Sitting);
            return View(await beanSceneContext.ToListAsync());
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Sitting)
                .FirstOrDefaultAsync(m => m.ReservationId == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create()
        {
            ViewData["SittingId"] = new SelectList(
        _context.SittingSchedules
        .Select(s => new
        {
            s.SittingScheduleId,
            Label = s.Stype + " ("
                + s.StartDateTime.ToString("h:mm tt")
                + " - "
                + s.EndDateTime.ToString("h:mm tt")
                + ")"
        }),
    "SittingScheduleId",
    "Label"
);

            return View();
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReservationId,SittingId,FirstName,LastName,Email,Phone,StartTime,Duration,NumOfGuests,ReservationSource,Notes,Status,CreatedAt")] Reservation reservation)
        {
            var sitting = await _context.SittingSchedules.FindAsync(reservation.SittingId);

            if (sitting == null)
                ModelState.AddModelError("SittingId", "Invalid sitting selected.");

            // ❌ Sitting is closed
            if (sitting.Status == "Closed")
                ModelState.AddModelError("SittingId", "This sitting is CLOSED. No reservations allowed.");

            // ❌ Over capacity
            var alreadyBooked = await _context.Reservations
                .Where(r => r.SittingId == reservation.SittingId)
                .SumAsync(r => (int?)r.NumOfGuests) ?? 0;

            if (alreadyBooked + reservation.NumOfGuests > sitting.Scapacity)
                ModelState.AddModelError("NumOfGuests", "Sitting capacity exceeded.");

            if (!ModelState.IsValid)
            {
                ViewData["SittingId"] = new SelectList(
                    _context.SittingSchedules.Select(s => new
                    {
                        s.SittingScheduleId,
                        Label = s.Stype + " (" +
                                s.StartDateTime.ToString("h:mm tt") + " - " +
                                s.EndDateTime.ToString("h:mm tt") + ")"
                    }),
                    "SittingScheduleId",
                    "Label",
                    reservation.SittingId
                );

                return View(reservation);
            }

            reservation.Status = "Pending";
            reservation.CreatedAt = DateTime.Now;

            _context.Add(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            ViewData["SittingId"] = new SelectList(
    _context.SittingSchedules.Select(s => new
    {
        s.SittingScheduleId,
        Label = s.Stype + " (" +
                s.StartDateTime.ToString("h:mm tt") + " - " +
                s.EndDateTime.ToString("h:mm tt") + ")"
    }),
    "SittingScheduleId",
    "Label",
    reservation.SittingId
);

            return View(reservation);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservationId,SittingId,FirstName,LastName,Email,Phone,StartTime,Duration,NumOfGuests,ReservationSource,Notes,Status,CreatedAt")] Reservation reservation)
        {
            if (id != reservation.ReservationId)
                return NotFound();

            var sitting = await _context.SittingSchedules.FindAsync(reservation.SittingId);

            if (sitting == null)
                ModelState.AddModelError("SittingId", "Invalid sitting selected.");

            // ❌ Sitting is closed
            if (sitting.Status == "Closed")
                ModelState.AddModelError("SittingId", "This sitting is CLOSED. No reservations allowed.");

            // ❌ Over capacity
            var alreadyBooked = await _context.Reservations
                .Where(r => r.SittingId == reservation.SittingId && r.ReservationId != id)
                .SumAsync(r => (int?)r.NumOfGuests) ?? 0;

            if (alreadyBooked + reservation.NumOfGuests > sitting.Scapacity)
                ModelState.AddModelError("NumOfGuests", "Sitting capacity exceeded.");

            if (!ModelState.IsValid)
            {
                ViewData["SittingId"] = new SelectList(
                    _context.SittingSchedules.Select(s => new
                    {
                        s.SittingScheduleId,
                        Label = s.Stype + " (" +
                                s.StartDateTime.ToString("h:mm tt") + " - " +
                                s.EndDateTime.ToString("h:mm tt") + ")"
                    }),
                    "SittingScheduleId",
                    "Label",
                    reservation.SittingId
                );

                return View(reservation);
            }

            try
            {
                _context.Update(reservation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Reservations.Any(r => r.ReservationId == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }



        // GET: Reservations/AssignTables/5
        public async Task<IActionResult> AssignTables(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations
                .Include(r => r.Sitting)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null) return NotFound();

            // find tables already assigned to this reservation
            var assignedIds = await _context.ReservationTables
                .Where(rt => rt.ReservationId == reservation.ReservationId)
                .Select(rt => rt.RestaurantTableID)
                .ToListAsync();

            // (simple version) show all tables – you can add extra filtering later
            var allTables = await _context.RestaurantTables
                .Include(t => t.Area)
                .OrderBy(t => t.TableName)
                .ToListAsync();

            var vm = new AssignTablesViewModel
            {
                ReservationId = reservation.ReservationId,
                GuestName = $"{reservation.FirstName} {reservation.LastName}",
                NumOfGuests = reservation.NumOfGuests,
                SittingName = reservation.Sitting.Stype,
                Status = reservation.Status,
                AvailableTables = allTables,
                SelectedTableIds = assignedIds
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTables(int id, AssignTablesViewModel model)
        {
            // Load the reservation (needed for SittingId)
            var reservation = await _context.Reservations
                .Include(r => r.Sitting)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound();

            // 1️⃣ Double booking check
            var conflict = await _context.ReservationTables
                .Include(rt => rt.Reservation)
                .Where(rt => model.SelectedTableIds.Contains(rt.RestaurantTableID)
                    && rt.Reservation.SittingId == reservation.SittingId
                    && rt.ReservationId != reservation.ReservationId)
                .ToListAsync();

            if (conflict.Any())
            {
                ModelState.AddModelError("", "One or more selected tables are already booked for this sitting.");

                model.AvailableTables = await _context.RestaurantTables
                    .Include(t => t.Area)
                    .OrderBy(t => t.TableName)
                    .ToListAsync();

                model.SelectedTableIds = await _context.ReservationTables
                    .Where(rt => rt.ReservationId == id)
                    .Select(rt => rt.RestaurantTableID)
                    .ToListAsync();

                return View(model);
            }

            // 2️⃣ Total seats check
            var selectedTables = await _context.RestaurantTables
                .Where(t => model.SelectedTableIds.Contains(t.RestaurantTableId))
                .ToListAsync();

            int totalSeats = selectedTables.Sum(t => t.Seats ?? 0);

            if (totalSeats < reservation.NumOfGuests)
            {
                TempData["SeatError"] =
                    $"Selected tables only seat {totalSeats}, but reservation requires {reservation.NumOfGuests}.";

                return RedirectToAction("AssignTables", new { id = id });
            }

            // 3️⃣ Remove old assignments
            var oldAssignments = _context.ReservationTables
                .Where(rt => rt.ReservationId == id);
            _context.ReservationTables.RemoveRange(oldAssignments);

            // 4️⃣ Save new assignments
            foreach (var tableId in model.SelectedTableIds)
            {
                _context.ReservationTables.Add(new ReservationTable
                {
                    ReservationId = id,
                    RestaurantTableID = tableId
                });
            }

            // 5️⃣ Update reservation status
            reservation.Status = "Confirmed";

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var reservation = await _context.Reservations
                .Include(r => r.Sitting)
                .FirstOrDefaultAsync(m => m.ReservationId == id);

            if (reservation == null)
                return NotFound();

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.ReservationId == id);
        }
    }
}
