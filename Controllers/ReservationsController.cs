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
using NuGet.Frameworks;


namespace BeanScene.Web.Controllers
{
    //[Authorize(Roles = "Admin,Staff")]
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

        /// <summary>
        /// GET: /Reservations/Index
        /// Retrieves and displays all reservations with their associated sitting information.
        /// Only accessible to Admin and Staff users.
        /// </summary>
        /// <returns>View displaying a list of all reservations including sitting details</returns>
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index()
        {
            // Query all reservations and eagerly load related Sitting data to avoid N+1 queries
            var beanSceneContext = _context.Reservations.Include(r => r.Sitting);
            return View(await beanSceneContext.ToListAsync());
        }

        /// <summary>
        /// GET: /Reservations/MyReservations
        /// Retrieves reservations belonging to the currently logged-in member user.
        /// Filters reservations by the user's email address.
        /// Only accessible to Member role users.
        /// </summary>
        /// <returns>View displaying reservations for the current user</returns>
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MyReservations()
        {
            // Get the current user's identity (email)
            var email = User.Identity?.Name; // Identity Email
            // Query reservations that match the current user's email and include related sitting data
            var reservations = await _context.Reservations
                .Where(r => r.Email == email)
                .Include(r => r.Sitting)
                .ToListAsync();

            return View(reservations);
        }


        /// <summary>
        /// GET: /Reservations/Details/5
        /// Retrieves and displays detailed information for a specific reservation.
        /// Only accessible to Admin and Staff users.
        /// </summary>
        /// <param name="id">The ID of the reservation to display</param>
        /// <returns>View showing detailed reservation information, or NotFound if reservation doesn't exist</returns>
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Details(int? id)
        {
            // Validate that an ID was provided
            if (id == null)
            {
                return NotFound();
            }

            // Query for the reservation by ID and eagerly load related sitting data
            var reservation = await _context.Reservations
                .Include(r => r.Sitting)
                .FirstOrDefaultAsync(m => m.ReservationId == id);
            
            // If reservation not found, return NotFound response
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        /// <summary>
        /// GET: /Reservations/Create
        /// Displays the form for creating a new reservation.
        /// Populates dropdown list with available sitting schedules formatted with type and time.
        /// Accessible to Members, Admins, and Staff.
        /// </summary>
        /// <returns>View with reservation creation form and sitting options</returns>
        [Authorize(Roles = "Member,Admin,Staff")]
        public IActionResult Create()
        {
            // Create a dropdown list of sitting schedules with formatted labels showing type and time range
            ViewData["SittingId"] = new SelectList(
        _context.SittingSchedules
        .Select(s => new
        {
            s.SittingScheduleId,
            // Format label as "Type (Start Time - End Time)" for user-friendly display
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

        /// <summary>
        /// POST: /Reservations/Create
        /// Processes form submission to create a new reservation.
        /// Performs validation: sitting availability, capacity constraints, and closed sittings.
        /// Sends confirmation email to customer if email is provided.
        /// </summary>
        /// <param name="reservation">The reservation object bound from the form</param>
        /// <returns>Redirects to Index (Admin/Staff) or Home (Member), or returns form if validation fails</returns>
        [Authorize(Roles = "Member,Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReservationId,SittingId,FirstName,LastName,Email,Phone,StartTime,Duration,NumOfGuests,ReservationSource,Notes,Status,CreatedAt")] Reservation reservation)
        {
            // Retrieve the sitting schedule to validate it exists
            var sitting = await _context.SittingSchedules.FindAsync(reservation.SittingId);

            // Validate that the selected sitting exists
            if (sitting == null)
                ModelState.AddModelError("SittingId", "Invalid sitting selected.");

            // Check if the sitting is closed and prevent reservations if so
            if (sitting.Status == "Closed")
                ModelState.AddModelError("SittingId", "This sitting is CLOSED. No reservations allowed.");

            // Calculate total guests already booked for this sitting to check capacity
            var alreadyBooked = await _context.Reservations
                .Where(r => r.SittingId == reservation.SittingId)
                .SumAsync(r => (int?)r.NumOfGuests) ?? 0;

            // Validate that adding this reservation doesn't exceed sitting capacity
            if (alreadyBooked + reservation.NumOfGuests > sitting.Scapacity)
                ModelState.AddModelError("NumOfGuests", "Sitting capacity exceeded.");

            // If validation failed, repopulate the form and return to view
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

            // Set default status to "Pending" and capture current timestamp
            reservation.Status = "Pending";
            reservation.CreatedAt = DateTime.Now;

            // Add reservation to database context
            _context.Add(reservation);
            await _context.SaveChangesAsync();

            // Send confirmation email to customer if email address provided
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
            
            // Redirect to home page for members, or back to reservations list for staff/admin
            if (User.IsInRole("Member"))
                return RedirectToAction("Index", "Home");

            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// GET: /Reservations/Edit/5
        /// Displays the form to edit an existing reservation.
        /// Members can only edit their own reservations; Admin/Staff can edit any reservation.
        /// </summary>
        /// <param name="id">The ID of the reservation to edit</param>
        /// <returns>View with reservation edit form, or Forbid if member accessing someone else's reservation</returns>
        [Authorize(Roles = "Member,Admin,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
                return NotFound();

            // Retrieve the reservation from database
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            // If user is a member, verify they can only edit their own reservation
            if (User.IsInRole("Member"))
            {
                var email = User.Identity?.Name;

                // Deny access if member tries to edit another user's reservation
                if (reservation.Email != email)
                    return Forbid();  // prevents editing other people's reservations
            }

            // Populate sitting schedule dropdown with formatted options
            ViewData["SittingId"] = new SelectList(
                _context.SittingSchedules.Select(s => new {
                    s.SittingScheduleId,
                    Label = s.Stype + " (" +
                           s.StartDateTime.ToString("h:mm tt") + " - " +
                           s.EndDateTime.ToString("h:mm tt") + ")"
                }),
                "SittingScheduleId", "Label", reservation.SittingId
            );

            return View(reservation);   
        }


        /// <summary>
        /// POST: /Reservations/Edit/5
        /// Processes form submission to update an existing reservation.
        /// Validates that ID matches, checks sitting availability and capacity.
        /// Admin and Staff only; Members cannot POST to this action.
        /// </summary>
        /// <param name="id">The ID of the reservation being edited</param>
        /// <param name="reservation">The updated reservation object from form</param>
        /// <returns>Redirects to MyReservations (Member) or Index (Staff/Admin), or returns form if validation fails</returns>
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservationId,SittingId,FirstName,LastName,Email,Phone,StartTime,Duration,NumOfGuests,ReservationSource,Notes,Status,CreatedAt")] Reservation reservation)
        {
            // If user is a member, verify they're editing their own reservation
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

            // Check if sitting is closed and prevent edits if so
            if (sitting.Status == "Closed")
                ModelState.AddModelError("SittingId", "This sitting is CLOSED. No reservations allowed.");

            // Calculate capacity: get all other reservations for this sitting (excluding current one)
            var alreadyBooked = await _context.Reservations
                .Where(r => r.SittingId == reservation.SittingId && r.ReservationId != id)
                .SumAsync(r => (int?)r.NumOfGuests) ?? 0;

            // Validate that updated guest count doesn't exceed capacity
            if (alreadyBooked + reservation.NumOfGuests > sitting.Scapacity)
                ModelState.AddModelError("NumOfGuests", "Sitting capacity exceeded.");

            // If validation failed, repopulate form and return to view
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
            
            // Redirect based on user role: members to their reservations, staff/admin to full list
            if (User.IsInRole("Member"))
            {
                return RedirectToAction("MyReservations");
            }

            return RedirectToAction(nameof(Index));

        }



        /// <summary>
        /// GET: /Reservations/AssignTables/5
        /// Displays form to assign restaurant tables to a reservation.
        /// Shows available tables and highlights already-assigned tables.
        /// Admin and Staff only.
        /// </summary>
        /// <param name="id">The ID of the reservation to assign tables to</param>
        /// <returns>View with table assignment form and available tables, or NotFound if reservation doesn't exist</returns>
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> AssignTables(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations
                .Include(r => r.Sitting)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null) return NotFound();

            // Get IDs of tables already assigned to this reservation
            var assignedIds = await _context.ReservationTables
                .Where(rt => rt.ReservationId == reservation.ReservationId)
                .Select(rt => rt.RestaurantTableID)
                .ToListAsync();

            // (simple version) show all tables – you can add extra filtering later
            var allTables = await _context.RestaurantTables
                .Include(t => t.Area)
                .OrderBy(t => t.TableName)
                .ToListAsync();

            // Create view model with reservation and table assignment details
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
        /// POST: /Reservations/AssignTables/5
        /// Processes table assignment for a reservation.
        /// Validates: no double-booking, sufficient seating capacity.
        /// Updates reservation status to "Confirmed" and sends confirmation email.
        /// </summary>
        /// <param name="id">The ID of the reservation</param>
        /// <param name="model">The assignment view model containing selected table IDs</param>
        /// <returns>Redirects to Index after successful assignment, or back to form if validation fails</returns>
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTables(int id, AssignTablesViewModel model)
        {
            model.SelectedTableIds ??= new List<int>();

            // Retrieve the reservation with sitting information (needed for validation)
            var reservation = await _context.Reservations
                .Include(r => r.Sitting)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound();

            // Check for double-booking: find tables already assigned to other reservations in the same sitting
            var conflict = await _context.ReservationTables
                .Include(rt => rt.Reservation)
                .Where(rt => model.SelectedTableIds.Contains(rt.RestaurantTableID)
                    && rt.Reservation.SittingId == reservation.SittingId
                    && rt.ReservationId != reservation.ReservationId)
                .ToListAsync();

            if (conflict.Any())
            {
                TempData["SeatError"] = "One or more of these selected tables are already booked for this sitting.";

                return RedirectToAction("AssignTables", new { id = id });

            }

            // Retrieve the selected table objects to calculate total seating capacity
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

            // Remove all existing table assignments for this reservation
            var oldAssignments = _context.ReservationTables
                .Where(rt => rt.ReservationId == id);
            _context.ReservationTables.RemoveRange(oldAssignments);

            // Create and add new table assignments for each selected table
            foreach (var tableId in model.SelectedTableIds)
            {
                _context.ReservationTables.Add(new ReservationTable
                {
                    ReservationId = id,
                    RestaurantTableID = tableId
                });
            }

            // Update reservation status to "Confirmed" to indicate tables have been assigned
            reservation.Status = "Confirmed";

            await _context.SaveChangesAsync();
            
            // Send confirmation email to customer with reservation details
            if (!string.IsNullOrEmpty(reservation.Email))
            {
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
            

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET: /Reservations/Delete/5
        /// Displays confirmation page before deleting a reservation.
        /// Shows reservation details to confirm deletion.
        /// Admin and Staff only.
        /// </summary>
        /// <param name="id">The ID of the reservation to delete</param>
        /// <returns>View showing reservation details for confirmation, or NotFound if not found</returns>
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
        /// POST: /Reservations/Delete/5
        /// Processes deletion of a reservation from the database.
        /// Members can only delete their own reservations; Admin/Staff can delete any.
        /// </summary>
        /// <param name="id">The ID of the reservation to delete</param>
        /// <returns>Redirects to MyReservations (Member) or Index (Admin/Staff)</returns>
        [Authorize(Roles = "Admin,Staff,Member")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                if (User.IsInRole("Memeber") && reservation.Email != User.Identity?.Name)
                    return Forbid();

                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }
            if (User.IsInRole("Member"))
                return RedirectToAction("MyReservations");

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method to check if a reservation exists in the database.
        /// </summary>
        /// <param name="id">The ID to check for</param>
        /// <returns>True if reservation with given ID exists; false otherwise</returns>
        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.ReservationId == id);
        }
    }
}
