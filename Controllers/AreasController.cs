using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeanScene.Web.Data;
using BeanScene.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BeanScene.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AreasController : Controller
    {
        private readonly BeanSceneContext _context;

        public AreasController(BeanSceneContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: /Areas/Index
        /// Retrieves and displays a list of all restaurant areas.
        /// Admin only.
        /// </summary>
        /// <returns>View displaying all areas</returns>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Areas.ToListAsync());
        }

        /// <summary>
        /// GET: /Areas/Details/5
        /// Retrieves and displays detailed information for a specific area.
        /// Admin only.
        /// </summary>
        /// <param name="id">The ID of the area to display</param>
        /// <returns>View showing area details, or NotFound if area doesn't exist</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Query for the specific area by ID
            var area = await _context.Areas
                .FirstOrDefaultAsync(m => m.AreaId == id);
            
            if (area == null)
            {
                return NotFound();
            }

            return View(area);
        }

        /// <summary>
        /// GET: /Areas/Create
        /// Displays the form for creating a new restaurant area.
        /// Admin only.
        /// </summary>
        /// <returns>View with area creation form</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST: /Areas/Create
        /// Processes form submission to create a new restaurant area.
        /// Validates model state and saves to database.
        /// </summary>
        /// <param name="area">The area object bound from the form</param>
        /// <returns>Redirects to Index on success, or returns form if validation fails</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AreaId,AreaName")] Area area)
        {
            // Check if all required fields are valid
            if (ModelState.IsValid)
            {
                _context.Add(area);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(area);
        }

        /// <summary>
        /// GET: /Areas/Edit/5
        /// Displays the form to edit an existing area.
        /// Admin only.
        /// </summary>
        /// <param name="id">The ID of the area to edit</param>
        /// <returns>View with area edit form, or NotFound if area doesn't exist</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            // Validate that an ID was provided
            if (id == null)
            {
                return NotFound();
            }

            var area = await _context.Areas.FindAsync(id);
            
            if (area == null)
            {
                return NotFound();
            }
            return View(area);
        }

        /// <summary>
        /// POST: /Areas/Edit/5
        /// Processes form submission to update an existing area.
        /// Validates ID match and model state before updating.
        /// </summary>
        /// <param name="id">The ID of the area being edited</param>
        /// <param name="area">The updated area object from form</param>
        /// <returns>Redirects to Index on success, or returns form if validation fails</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AreaId,AreaName")] Area area)
        {
            if (id != area.AreaId)
            {
                return NotFound();
            }

            // Check if all required fields are valid
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(area);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency conflicts if area was deleted by another user
                    if (!AreaExists(area.AreaId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(area);
        }

        /// <summary>
        /// GET: /Areas/Delete/5
        /// Displays confirmation page before deleting an area.
        /// Shows area details to confirm deletion.
        /// Admin only.
        /// </summary>
        /// <param name="id">The ID of the area to delete</param>
        /// <returns>View showing area details for confirmation, or NotFound if not found</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var area = await _context.Areas
                .FirstOrDefaultAsync(m => m.AreaId == id);
            
            if (area == null)
            {
                return NotFound();
            }

            return View(area);
        }

        /// <summary>
        /// POST: /Areas/Delete/5
        /// Processes deletion of an area from the database.
        /// </summary>
        /// <param name="id">The ID of the area to delete</param>
        /// <returns>Redirects to Index after deletion</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var area = await _context.Areas.FindAsync(id);
            
            if (area != null)
            {
                _context.Areas.Remove(area);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
            // Redirect to the areas list
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method to check if an area exists in the database.
        /// </summary>
        /// <param name="id">The ID to check for</param>
        /// <returns>True if area with given ID exists; false otherwise</returns>
        private bool AreaExists(int id)
        {
            return _context.Areas.Any(e => e.AreaId == id);
        }
    }
}
