using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeanScene.Web.Data;
using BeanScene.Web.Models;
using BeanScene.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BeanScene.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SittingSchedulesController : Controller
    {
        private readonly BeanSceneContext _context;
        private readonly SittingScheduleService _sittingService;

        public SittingSchedulesController(BeanSceneContext context, SittingScheduleService sittingSchedule)
        {
            _context = context;
            _sittingService = sittingSchedule;
        }

        /// <summary>
        /// GET: /SittingSchedules/Index
        /// Retrieves and displays all sitting schedules configured for the restaurant.
        /// Admin only.
        /// </summary>
        /// <returns>View displaying list of all sitting schedules</returns>
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            await _sittingService.EnsureDailySittingsExistAsync(today);

            var sittings  = await _context.SittingSchedules
                .AsNoTracking()
                .Include(s => s.Reservations)
                .Where(s =>
                    s.StartDateTime.Date == today.Date ||
                    s.Status == SittingStatus.Closed ||
                    s.Reservations.Any())
                .OrderBy(s => s.StartDateTime)
                .ToListAsync();

            return View(sittings);
        }

        /// <summary>
        /// GET: /SittingSchedules/Details/5
        /// Retrieves and displays detailed information for a specific sitting schedule.
        /// Admin only.
        /// </summary>
        /// <param name="id">The ID of the sitting schedule to display</param>
        /// <returns>View showing sitting schedule details, or NotFound if not found</returns>
        public async Task<IActionResult> Details(int? id)
        {
            // Validate that an ID was provided
            if (id == null)
            {
                return NotFound();
            }

            // Query for the specific sitting schedule by ID
            var sittingSchedule = await _context.SittingSchedules
                .FirstOrDefaultAsync(m => m.SittingScheduleId == id);
            
            // Return NotFound if sitting schedule doesn't exist
            if (sittingSchedule == null)
            {
                return NotFound();
            }

            return View(sittingSchedule);
        }

        /// <summary>
        /// GET: /SittingSchedules/Create
        /// Displays the form for creating a new sitting schedule.
        /// Admin only.
        /// </summary>
        /// <returns>View with sitting schedule creation form</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST: /SittingSchedules/Create
        /// Processes form submission to create a new sitting schedule.
        /// Validates model state and saves to database.
        /// </summary>
        /// <param name="sittingSchedule">The sitting schedule object bound from the form</param>
        /// <returns>Redirects to Index on success, or returns form if validation fails</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SittingScheduleId,Stype,StartDateTime,EndDateTime,Scapacity,Status")] SittingSchedule sittingSchedule)
        {
            // Check if all required fields are valid
            if (ModelState.IsValid)
            {
                // Add the new sitting schedule to the database context
                _context.Add(sittingSchedule);
                // Save changes to the database
                await _context.SaveChangesAsync();
                // Redirect to the sitting schedules list
                return RedirectToAction(nameof(Index));
            }
            // Return to form if validation failed
            return View(sittingSchedule);
        }

        /// <summary>
        /// GET: /SittingSchedules/Edit/5
        /// Displays the form to edit an existing sitting schedule.
        /// Admin only.
        /// </summary>
        /// <param name="id">The ID of the sitting schedule to edit</param>
        /// <returns>View with sitting schedule edit form, or NotFound if not found</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            // Validate that an ID was provided
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the sitting schedule from database by ID
            var sittingSchedule = await _context.SittingSchedules.FindAsync(id);
            
            // Return NotFound if sitting schedule doesn't exist
            if (sittingSchedule == null)
            {
                return NotFound();
            }
            return View(sittingSchedule);
        }

        /// <summary>
        /// POST: /SittingSchedules/Edit/5
        /// Processes form submission to update an existing sitting schedule.
        /// Validates ID match and model state before updating.
        /// </summary>
        /// <param name="id">The ID of the sitting schedule being edited</param>
        /// <param name="sittingSchedule">The updated sitting schedule object from form</param>
        /// <returns>Redirects to Index on success, or returns form if validation fails</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SittingScheduleId,Stype,StartDateTime,EndDateTime,Scapacity,Status")] SittingSchedule sittingSchedule)
        {
            // Validate that provided ID matches the sitting schedule's ID
            if (id != sittingSchedule.SittingScheduleId)
            {
                return NotFound();
            }

            // Check if all required fields are valid
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the sitting schedule in the database context
                    _context.Update(sittingSchedule);
                    // Save changes to the database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency conflicts if sitting schedule was deleted by another user
                    if (!SittingScheduleExists(sittingSchedule.SittingScheduleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect to the sitting schedules list after successful update
                return RedirectToAction(nameof(Index));
            }
            // Return to form if validation failed
            return View(sittingSchedule);
        }

        /// <summary>
        /// GET: /SittingSchedules/Delete/5
        /// Displays confirmation page before deleting a sitting schedule.
        /// Shows sitting schedule details to confirm deletion.
        /// Admin only.
        /// </summary>
        /// <param name="id">The ID of the sitting schedule to delete</param>
        /// <returns>View showing sitting schedule details for confirmation, or NotFound if not found</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            // Validate that an ID was provided
            if (id == null)
            {
                return NotFound();
            }

            // Query for the specific sitting schedule by ID
            var sittingSchedule = await _context.SittingSchedules
                .FirstOrDefaultAsync(m => m.SittingScheduleId == id);
            
            // Return NotFound if sitting schedule doesn't exist
            if (sittingSchedule == null)
            {
                return NotFound();
            }

            return View(sittingSchedule);
        }

        /// <summary>
        /// POST: /SittingSchedules/Delete/5
        /// Processes deletion of a sitting schedule from the database.
        /// </summary>
        /// <param name="id">The ID of the sitting schedule to delete</param>
        /// <returns>Redirects to Index after deletion</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Retrieve the sitting schedule from database by ID
            var sittingSchedule = await _context.SittingSchedules.FindAsync(id);
            
            // If sitting schedule found, remove it from the database context
            if (sittingSchedule != null)
            {
                _context.SittingSchedules.Remove(sittingSchedule);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
            // Redirect to the sitting schedules list
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method to check if a sitting schedule exists in the database.
        /// </summary>
        /// <param name="id">The ID to check for</param>
        /// <returns>True if sitting schedule with given ID exists; false otherwise</returns>
        private bool SittingScheduleExists(int id)
        {
            return _context.SittingSchedules.Any(e => e.SittingScheduleId == id);
        }

    }
}
