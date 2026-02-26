using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeanScene.Web.Data;
using BeanScene.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BeanScene.Web.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly BeanSceneContext _context;
        private readonly IEmailSender _emailSender;

        public ReservationsController(BeanSceneContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // ── Actions ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Lists all reservations with their associated sitting.
        /// </summary>
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index()
        {
            var beanSceneContext = _context.Reservations.Include(r => r.Sitting);
            return View(await beanSceneContext.ToListAsync());
        }

        /// <summary>
        /// Lists reservations for the currently logged-in member, matched by email address.
        /// </summary>
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MyReservations()
        {
            var email = GetCurrentUserEmail();
            var reservations = await _context.Reservations
                .Where(r => r.Email == email)
                .Include(r => r.Sitting)
                .ToListAsync();

            return View(reservations);
        }

        /// <summary>
        /// Shows full details for a single reservation.
        /// </summary>
        /// <param name="id">Reservation ID.</param>
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Details(int? id)
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

        /// <summary>
        /// Displays the reservation creation form.
        /// Sitting dropdown labels are formatted as "Type (Start – End)".
        /// </summary>
        [Authorize(Roles = "Member,Admin,Staff")]
        public IActionResult Create()
        {
            ViewData["SittingId"] = BuildSittingSelectList();              
            return View();
        }

        /// <summary>
        /// Creates a new reservation after validating that the sitting is Open and has sufficient capacity.
        /// Sends a confirmation email on success. Members are redirected to Home; Admin/Staff to Index.
        /// </summary>
        /// <param name="reservation">Reservation bound from the form.</param>
        [Authorize(Roles = "Member,Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReservationId,SittingId,FirstName,LastName,Email,Phone,StartTime,Duration,NumOfGuests,ReservationSource,Notes,Status,CreatedAt")] Reservation reservation)
        {
            var sitting = await GetSittingOrAddModelErrorAsync(reservation.SittingId);  
            if (sitting != null)
            {
                ValidateSittingIsOpen(sitting);                            
                await ValidateSittingCapacityAsync(sitting, reservation.NumOfGuests);   
            }

            if (!ModelState.IsValid)
            {
                ViewData["SittingId"] = BuildSittingSelectList(reservation.SittingId); 
                return View(reservation);
            }

            reservation.Status = "Pending";
            reservation.CreatedAt = DateTime.Now;

            _context.Add(reservation);
            await _context.SaveChangesAsync();     
            
            if (sitting !=null)
            {
                await SendCreatedEmailAsync(reservation, sitting!);
            }

            if (IsMember())                                                
                return RedirectToAction("Index", "Home");

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays the reservation edit form. Members may only edit their own reservations.
        /// </summary>
        /// <param name="id">Reservation ID.</param>
        /// <returns>Edit form, or <see cref="ForbidResult"/> if a Member accesses another user's reservation.</returns>
        [Authorize(Roles = "Member,Admin,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            // Members may only edit their own reservations.
            if (IsMember() && reservation.Email != GetCurrentUserEmail()) // step 3
                return Forbid();

            ViewData["SittingId"] = BuildSittingSelectList(reservation.SittingId);     // step 2
            return View(reservation);
        }

        /// <summary>
        /// Saves edits to an existing reservation after validating the sitting is Open and has capacity.
        /// Members may only edit their own reservations. Capacity check excludes the current reservation.
        /// Members are redirected to MyReservations; Admin/Staff to Index.
        /// </summary>
        /// <param name="id">Reservation ID from route.</param>
        /// <param name="reservation">Updated reservation bound from the form.</param>
        [Authorize(Roles = "Member,Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservationId,SittingId,FirstName,LastName,Email,Phone,StartTime,Duration,NumOfGuests,ReservationSource,Notes,Status,CreatedAt")] Reservation reservation)
        {
            // Members may only edit their own reservations.
            if (IsMember() && reservation.Email != GetCurrentUserEmail()) // step 3
                return Forbid();

            if (id != reservation.ReservationId)
                return NotFound();

            var sitting = await GetSittingOrAddModelErrorAsync(reservation.SittingId);  // step 4
            if (sitting != null)
            {
                ValidateSittingIsOpen(sitting);                            // step 4
                // Exclude the current reservation so it doesn't count against its own sitting capacity.
                await ValidateSittingCapacityAsync(sitting, reservation.NumOfGuests, id); // step 4
            }

            if (!ModelState.IsValid)
            {
                ViewData["SittingId"] = BuildSittingSelectList(reservation.SittingId); // step 2
                return View(reservation);
            }

            try
            {
                _context.Update(reservation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(id))
                    return NotFound();
                throw;
            }

            if (IsMember())                                                // step 3
                return RedirectToAction("MyReservations");

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays the table assignment form for a reservation, pre-selecting any already-assigned tables.
        /// All tables are shown; availability filtering within the sitting is not yet applied.
        /// </summary>
        /// <param name="id">Reservation ID.</param>
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> AssignTables(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations
                .Include(r => r.Sitting)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null) return NotFound();

            var assignedIds = await _context.ReservationTables
                .Where(rt => rt.ReservationId == reservation.ReservationId)
                .Select(rt => rt.RestaurantTableID)
                .ToListAsync();

            // Simple version: shows all tables. Extra filtering (e.g. by area or capacity) can be added later.
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

        /// <summary>
        /// Assigns tables to a reservation and marks it Confirmed.
        /// Validates that no selected table is already booked for this sitting, and that
        /// combined seat count meets the guest count. Sends a confirmation email on success.
        /// Existing table assignments are fully replaced.
        /// </summary>
        /// <param name="id">Reservation ID from route.</param>
        /// <param name="model">View model containing the selected table IDs.</param>
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTables(int id, AssignTablesViewModel model)
        {
            model.SelectedTableIds ??= new List<int>();

            var reservation = await _context.Reservations
                .Include(r => r.Sitting)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound();

            // Double-booking check: reject any table already assigned to another reservation in this sitting.
            if (await HasTableConflictsAsync(reservation, model.SelectedTableIds))  // step 6
            {
                TempData["SeatError"] = "One or more of these selected tables are already booked for this sitting.";
                return RedirectToAction("AssignTables", new { id = id });
            }

            int totalSeats = await GetTotalSeatsAsync(model.SelectedTableIds);      // step 6
            if (totalSeats < reservation.NumOfGuests)
            {
                TempData["SeatError"] =
                    $"Selected tables only seat {totalSeats}, but reservation requires {reservation.NumOfGuests}.";
                return RedirectToAction("AssignTables", new { id = id });
            }

            // Replace all existing table assignments for this reservation.
            var oldAssignments = _context.ReservationTables.Where(rt => rt.ReservationId == id);
            _context.ReservationTables.RemoveRange(oldAssignments);

            foreach (var tableId in model.SelectedTableIds)
            {
                _context.ReservationTables.Add(new ReservationTable
                {
                    ReservationId = id,
                    RestaurantTableID = tableId
                });
            }

            // Assigning tables moves the reservation to Confirmed status.
            reservation.Status = "Confirmed";
            await _context.SaveChangesAsync();

            await SendConfirmedEmailAsync(reservation);                    // step 5

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays the delete confirmation page for a reservation.
        /// </summary>
        /// <param name="id">Reservation ID.</param>
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

        /// <summary>
        /// Deletes a reservation. Members may only delete their own; Admin/Staff can delete any.
        /// </summary>
        /// <param name="id">Reservation ID.</param>
        [Authorize(Roles = "Admin,Staff,Member")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                if (IsMember() && reservation.Email != GetCurrentUserEmail()) // step 3
                    return Forbid();

                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }

            if (IsMember())                                                // step 3
                return RedirectToAction("MyReservations");

            return RedirectToAction(nameof(Index));
        }

        // ── Dropdown helper ───────────────────────────────────────────────────────

        /// <summary>Builds the sitting schedule <see cref="SelectList"/> used in Create and Edit forms.</summary>
        private SelectList BuildSittingSelectList(int? selectedId = null)
        {
            var items = _context.SittingSchedules
                .AsNoTracking() // read-only query
                .Select(s => new
                {
                    s.SittingScheduleId,
                    Label = s.Stype + " (" +
                            s.StartDateTime.ToString("h:mm tt") + " - " +
                            s.EndDateTime.ToString("h:mm tt") + ")"
                })
                .ToList(); // force query execution now

            return new SelectList(items, "SittingScheduleId", "Label", selectedId);
        }

        // ── User identity helpers ─────────────────────────────────────────────────

        private string? GetCurrentUserEmail() => User.Identity?.Name;
        private bool IsMember() => User.IsInRole("Member");

        // ── Sitting validation helpers ────────────────────────────────────────────

        /// <summary>Loads the sitting by ID; adds a model error and returns null if not found.</summary>
        private async Task<SittingSchedule?> GetSittingOrAddModelErrorAsync(int sittingId)
        {
            var sitting = await _context.SittingSchedules.FindAsync(sittingId);
            if (sitting == null)
                ModelState.AddModelError("SittingId", "Invalid sitting selected.");
            return sitting;
        }

        private void ValidateSittingIsOpen(SittingSchedule sitting)
        {
            if (sitting.Status == "Closed")
                ModelState.AddModelError("SittingId", "This sitting is CLOSED. No reservations allowed.");
        }

        /// <param name="excludeReservationId">Exclude this reservation from the booked-guest count (used on edits).</param>
        private async Task ValidateSittingCapacityAsync(
            SittingSchedule sitting, int guests, int? excludeReservationId = null)
        {
            var alreadyBooked = await _context.Reservations
                .Where(r => r.SittingId == sitting.SittingScheduleId &&
                            (excludeReservationId == null || r.ReservationId != excludeReservationId))
                .SumAsync(r => (int?)r.NumOfGuests) ?? 0;

            if (alreadyBooked + guests > sitting.Scapacity)
                ModelState.AddModelError("NumOfGuests", "Sitting capacity exceeded.");
        }

        // ── Email helpers ─────────────────────────────────────────────────────────

        private async Task SendCreatedEmailAsync(Reservation reservation, SittingSchedule sitting)
        {
            if (string.IsNullOrEmpty(reservation.Email)) return;
            var subject = "Reservation Created";
            var message = $"Dear {reservation.FirstName} {reservation.LastName},<br/><br/>" +
                          $"Your reservation for {reservation.NumOfGuests} guests on {sitting.Stype} sitting " +
                          $"at {reservation.StartTime:h:mm tt} has been created successfully.<br/><br/>" +
                          "Thank you for choosing our restaurant!<br/><br/>" +
                          "Best regards,<br/>BeanScene Team";
            await _emailSender.SendEmailAsync(reservation.Email, subject, message);
        }

        private async Task SendConfirmedEmailAsync(Reservation reservation)
        {
            if (string.IsNullOrEmpty(reservation.Email)) return;
            var subject = "Your Reservation is Confirmed";
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
            await _emailSender.SendEmailAsync(reservation.Email, subject, confirmMessage);
        }

        // ── AssignTables helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Returns true if any selected table is already booked for this sitting under a different reservation.
        /// </summary>
        private async Task<bool> HasTableConflictsAsync(Reservation reservation, List<int> selectedTableIds)
        {
            return await _context.ReservationTables
                .Include(rt => rt.Reservation)
                .Where(rt => selectedTableIds.Contains(rt.RestaurantTableID)
                    && rt.Reservation.SittingId == reservation.SittingId
                    && rt.ReservationId != reservation.ReservationId)
                .AnyAsync();
        }

        /// <summary>Returns the total seat count across the given table IDs.</summary>
        private async Task<int> GetTotalSeatsAsync(List<int> selectedTableIds)
        {
            var tables = await _context.RestaurantTables
                .Where(t => selectedTableIds.Contains(t.RestaurantTableId))
                .ToListAsync();
            return tables.Sum(t => t.Seats ?? 0);
        }

        // ── Misc ──────────────────────────────────────────────────────────────────

        /// <summary>Returns true if a reservation with the given ID exists.</summary>
        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.ReservationId == id);
        }
    }
}
