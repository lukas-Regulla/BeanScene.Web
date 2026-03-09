using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeanScene.Web.Data;
using BeanScene.Web.Models;
using BeanScene.Web.Services;
using Microsoft.AspNetCore.Authorization;

namespace BeanScene.Web.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly BeanSceneContext _context;
        private readonly IReservationValidator _validator;
        private readonly IReservationEmailService _emailService;
        private readonly ITableAssignmentService _tableAssignment;
        private readonly SittingScheduleService _sittingScheduleService;

        public ReservationsController(BeanSceneContext context, IReservationValidator validator, IReservationEmailService emailService, ITableAssignmentService tableAssignment, SittingScheduleService sittingScheduleService)
        {
            _context = context;
            _validator = validator;
            _emailService = emailService;
            _tableAssignment = tableAssignment;
            _sittingScheduleService = sittingScheduleService;
        }

        // ── Actions ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Lists all reservations with their associated sitting.
        /// </summary>
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index()
        {
            var beanSceneContext = _context.Reservations
                .Include(r => r.Sitting)
                .OrderByDescending(r => r.CreatedAt);
            return View(await beanSceneContext.ToListAsync());
        }

        /// <summary>
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
                .OrderByDescending(r => r.CreatedAt)
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
            ViewData["Stype"] = new SelectList(new[] { "Breakfast","Lunch", "Dinner" });              
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
        public async Task<IActionResult> Create([Bind("ReservationId,Stype,FirstName,LastName,Email,Phone,StartTime,Duration,NumOfGuests,ReservationSource,Notes,Status,CreatedAt")] Reservation reservation)
        {
            var sittingDate = reservation.StartTime.Date;

            var sitting = await _sittingScheduleService.GetOrCreateSittingAsync(sittingDate, reservation.Stype);
            reservation.SittingId = sitting.SittingScheduleId;

            var (_, validation) = await _validator.ValidateCreateAsync(
                reservation.SittingId,
                reservation.NumOfGuests,
                reservation.StartTime);

            foreach (var (key, msg) in validation.Errors)
                ModelState.AddModelError(key, msg);

            if (!ModelState.IsValid)
            {
                ViewData["Stype"] = new SelectList(new[] { "Breakfast", "Lunch", "Dinner" }, reservation.Stype);
                return View(reservation);
            }

            reservation.Status = ReservationStatus.Pending;
            reservation.CreatedAt = DateTime.Now;

            _context.Add(reservation);
            await _context.SaveChangesAsync();

            await _emailService.SendCreatedAsync(reservation, sitting);

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

            var (_, validation) = await _validator.ValidateEditAsync(reservation.SittingId, reservation.NumOfGuests, id, reservation.StartTime);
            foreach (var (key, msg) in validation.Errors)
                ModelState.AddModelError(key, msg);

            if (!ModelState.IsValid)
            {
                ViewData["SittingId"] = BuildSittingSelectList(reservation.SittingId);
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

            if (reservation == null) return NotFound();

            var result = await _tableAssignment.AssignAsync(reservation, model.SelectedTableIds);

            if (!result.Succeeded)
            {
                TempData["SeatError"] = result.ErrorMessage;
                return RedirectToAction("AssignTables", new { id });
            }

            await _emailService.SendConfirmedAsync(reservation);
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
                .AsNoTracking()
                .Where(s => s.StartDateTime.Date >= DateTime.Today
                         && s.Status == SittingStatus.Open)
                .OrderBy(s => s.StartDateTime)
                .Select(s => new
                {
                    s.SittingScheduleId,
                    Label = s.Stype + " — " + s.StartDateTime.ToString("ddd d MMM") +
                            " (" + s.StartDateTime.ToString("h:mm tt") + " – " +
                            s.EndDateTime.ToString("h:mm tt") + ")"
                })
                .ToList();

            return new SelectList(items, "SittingScheduleId", "Label", selectedId);
        }

        // ── User identity helpers ─────────────────────────────────────────────────

        private string? GetCurrentUserEmail() => User.Identity?.Name;
        private bool IsMember() => User.IsInRole("Member");

        // ── Misc ──────────────────────────────────────────────────────────────────

        /// <summary>Returns true if a reservation with the given ID exists.</summary>
        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.ReservationId == id);
        }
    }
}
