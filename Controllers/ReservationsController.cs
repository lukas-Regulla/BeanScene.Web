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
using Microsoft.AspNetCore.Identity.UI.Services;


namespace BeanScene.Web.Controllers
{
    //[Authorize(Roles = "Admin,Staff")]
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly BeanSceneContext _context;

        //public ReservationsController(BeanSceneContext context)
        //{
        //    _context = context;

        //}
        private readonly IEmailSender _emailSender;
        public ReservationsController(BeanSceneContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }




        // GET: Reservations
        [Authorize(Roles = "Member,Admin,Staff")]
        public async Task<IActionResult> Index()
        {
            var beanSceneContext = _context.Reservations.Include(r => r.Sitting);
            return View(await beanSceneContext.ToListAsync());
        }
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MyReservations()
        {
            var email = User.Identity?.Name; // Identity Email
            var reservations = await _context.Reservations
                .Where(r => r.Email == email)
                .Include(r => r.Sitting)
                .ToListAsync();

            return View(reservations);
        }


        // GET: Reservations/Details/5
        [Authorize(Roles = "Admin,Staff")]
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
        [Authorize(Roles = "Member,Admin,Staff")]
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
        [Authorize(Roles = "Member,Admin,Staff")]
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

            if (!string.IsNullOrEmpty(reservation.Email))
            {
                var subject = "Reservation Created";
                var message = $"Dear {reservation.FirstName} {reservation.LastName},<br/><br/>" +
                              $"Your reservation for {reservation.NumOfGuests} guests on {sitting.Stype} sitting " +
                              $"at {reservation.StartTime:h:mm tt} has been created successfully.<br/><br/>" +
                              "Thank you for choosing our restaurant!<br/><br/>" +
                              "Best regards,<br/>BeanScene Team";
                await _emailSender.SendEmailAsync(reservation.Email, subject, message);
            }
            return RedirectToAction(nameof(Index));
        }


        // GET: Reservations/Edit/5
        [Authorize(Roles = "Member,Admin,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            // If a member, they can only edit *their* reservation
            if (User.IsInRole("Member"))
            {
                var email = User.Identity?.Name;

                if (reservation.Email != email)
                    return Forbid();  // prevents editing other people's reservations
            }

            ViewData["SittingId"] = new SelectList(
                _context.SittingSchedules.Select(s => new {
                    s.SittingScheduleId,
                    Label = s.Stype + " (" +
                           s.StartDateTime.ToString("h:mm tt") + " - " +
                           s.EndDateTime.ToString("h:mm tt") + ")"
                }),
                "SittingScheduleId", "Label", reservation.SittingId
            );

            return View(reservation);   // <-- THIS WAS MISSING
        }


        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservationId,SittingId,FirstName,LastName,Email,Phone,StartTime,Duration,NumOfGuests,ReservationSource,Notes,Status,CreatedAt")] Reservation reservation)
        {
            // If Member, ensure it’s their reservation
            if (User.IsInRole("Member"))
            {
                var email = User.Identity?.Name;
                if (reservation.Email != email)
                    return Forbid();
            }

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
            if (User.IsInRole("Member"))
            {
                return RedirectToAction("MyReservations");
            }

            return RedirectToAction(nameof(Index));

        }



        // GET: Reservations/AssignTables/5
        [Authorize(Roles = "Admin,Staff")]
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
        [Authorize(Roles = "Admin,Staff")]
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

            // 1️ Double booking check
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

            // 2️ Total seats check
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

            // 3️ Remove old assignments
            var oldAssignments = _context.ReservationTables
                .Where(rt => rt.ReservationId == id);
            _context.ReservationTables.RemoveRange(oldAssignments);

            // 4️ Save new assignments
            foreach (var tableId in model.SelectedTableIds)
            {
                _context.ReservationTables.Add(new ReservationTable
                {
                    ReservationId = id,
                    RestaurantTableID = tableId
                });
            }

            // 5️ Update reservation status
            reservation.Status = "Confirmed";

            await _context.SaveChangesAsync();
            // 📧 Send "Confirmed" email to customer
            if (!string.IsNullOrEmpty(reservation.Email))
            {
                var confirmMessage = $@"
            <h2>Your Reservation is Confirmed!</h2>
            <p>Hi {reservation.FirstName},</p>
            <p>Your reservation at BeanScene Café has been confirmed.</p>

            <h3>Reservation Details</h3>
            <p><strong>Date:</strong> {reservation.StartTime:dddd, dd MMM yyyy}</p>
            <p><strong>Start Time:</strong> {reservation.StartTime:hh:mm tt}</p>
            <p><strong>Guests:</strong> {reservation.NumOfGuests}</p>
            <p><strong>Duration:</strong> {reservation.Duration} minutes</p>
            <p><strong>Status:</strong> Confirmed</p>

            <p>We look forward to seeing you!</p>
        ";


            }

            return RedirectToAction(nameof(Index));
        }
        // GET: Reservations/Delete/5
        [Authorize(Roles = "Admin,Staff")]
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
        [Authorize(Roles = "Admin,Staff")]
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
