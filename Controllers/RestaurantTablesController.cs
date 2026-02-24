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
    public class RestaurantTablesController : Controller
    {
        private readonly BeanSceneContext _context;

        public RestaurantTablesController(BeanSceneContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: /RestaurantTables/Index
        /// Retrieves and displays all restaurant tables with their associated area information.
        /// Admin only.
        /// </summary>
        /// <returns>View displaying list of all restaurant tables with areas</returns>
        public async Task<IActionResult> Index()
        {
            // Query all restaurant tables and eagerly load their related Area data to avoid N+1 queries
            var beanSceneContext = _context.RestaurantTables.Include(r => r.Area);
            return View(await beanSceneContext.ToListAsync());
        }

        /// <summary>
        /// GET: /RestaurantTables/Details/5
        /// Retrieves and displays detailed information for a specific restaurant table.
        /// Admin only.
        /// </summary>
        /// <param name="id">The ID of the restaurant table to display</param>
        /// <returns>View showing table details with area information, or NotFound if not found</returns>
        public async Task<IActionResult> Details(int? id)
        {
            // Validate that an ID was provided
            if (id == null)
            {
                return NotFound();
            }

            // Query for the specific table by ID and eagerly load its related area
            var restaurantTable = await _context.RestaurantTables
                .Include(r => r.Area)
                .FirstOrDefaultAsync(m => m.RestaurantTableId == id);
            
            // Return NotFound if table doesn't exist
            if (restaurantTable == null)
            {
                return NotFound();
            }

            return View(restaurantTable);
        }

        /// <summary>
        /// GET: /RestaurantTables/Create
        /// Displays the form for creating a new restaurant table.
        /// Populates dropdown list with available areas.
        /// Admin only.
        /// </summary>
        /// <returns>View with table creation form and area options</returns>
        public IActionResult Create()
        {
            // Create a dropdown list of all areas for table assignment
            ViewData["AreaId"] = new SelectList(_context.Areas, "AreaId", "AreaName");
            return View();
        }

        /// <summary>
        /// POST: /RestaurantTables/Create
        /// Processes form submission to create a new restaurant table.
        /// Validates model state and saves to database.
        /// </summary>
        /// <param name="restaurantTable">The table object bound from the form</param>
        /// <returns>Redirects to Index on success, or returns form if validation fails</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RestaurantTableId,AreaId,TableName,Seats")] RestaurantTable restaurantTable)
        {
            // Check if all required fields are valid
            if (ModelState.IsValid)
            {
                // Add the new table to the database context
                _context.Add(restaurantTable);
                // Save changes to the database
                await _context.SaveChangesAsync();
                // Redirect to the tables list
                return RedirectToAction(nameof(Index));
            }
            // Repopulate area dropdown and return to form if validation failed
            ViewData["AreaId"] = new SelectList(_context.Areas, "AreaId", "AreaName", restaurantTable.AreaId);
            return View(restaurantTable);
        }

        /// <summary>
        /// GET: /RestaurantTables/Edit/5
        /// Displays the form to edit an existing restaurant table.
        /// Populates dropdown list with available areas.
        /// Admin only.
        /// </summary>
        /// <param name="id">The ID of the table to edit</param>
        /// <returns>View with table edit form and area options, or NotFound if not found</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            // Validate that an ID was provided
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the table from database by ID
            var restaurantTable = await _context.RestaurantTables.FindAsync(id);
            
            // Return NotFound if table doesn't exist
            if (restaurantTable == null)
            {
                return NotFound();
            }
            
            // Create area dropdown with the table's current area selected
            ViewData["AreaId"] = new SelectList(_context.Areas, "AreaId", "AreaName", restaurantTable.AreaId);
            return View(restaurantTable);
        }

        /// <summary>
        /// POST: /RestaurantTables/Edit/5
        /// Processes form submission to update an existing restaurant table.
        /// Validates ID match and model state before updating.
        /// </summary>
        /// <param name="id">The ID of the table being edited</param>
        /// <param name="restaurantTable">The updated table object from form</param>
        /// <returns>Redirects to Index on success, or returns form if validation fails</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RestaurantTableId,AreaId,TableName,Seats")] RestaurantTable restaurantTable)
        {
            // Validate that provided ID matches the table's ID
            if (id != restaurantTable.RestaurantTableId)
            {
                return NotFound();
            }

            // Check if all required fields are valid
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the table in the database context
                    _context.Update(restaurantTable);
                    // Save changes to the database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency conflicts if table was deleted by another user
                    if (!RestaurantTableExists(restaurantTable.RestaurantTableId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect to the tables list after successful update
                return RedirectToAction(nameof(Index));
            }
            
            // Repopulate area dropdown and return to form if validation failed
            ViewData["AreaId"] = new SelectList(_context.Areas, "AreaId", "AreaId", restaurantTable.AreaId);
            return View(restaurantTable);
        }

        /// <summary>
        /// GET: /RestaurantTables/Delete/5
        /// Displays confirmation page before deleting a restaurant table.
        /// Shows table details with area information to confirm deletion.
        /// Admin only.
        /// </summary>
        /// <param name="id">The ID of the table to delete</param>
        /// <returns>View showing table details for confirmation, or NotFound if not found</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            // Validate that an ID was provided
            if (id == null)
            {
                return NotFound();
            }

            // Query for the specific table by ID and include its area information
            var restaurantTable = await _context.RestaurantTables
                .Include(r => r.Area)
                .FirstOrDefaultAsync(m => m.RestaurantTableId == id);
            
            // Return NotFound if table doesn't exist
            if (restaurantTable == null)
            {
                return NotFound();
            }

            return View(restaurantTable);
        }

        /// <summary>
        /// POST: /RestaurantTables/Delete/5
        /// Processes deletion of a restaurant table from the database.
        /// </summary>
        /// <param name="id">The ID of the table to delete</param>
        /// <returns>Redirects to Index after deletion</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Retrieve the table from database by ID
            var restaurantTable = await _context.RestaurantTables.FindAsync(id);
            
            // If table found, remove it from the database context
            if (restaurantTable != null)
            {
                _context.RestaurantTables.Remove(restaurantTable);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
            // Redirect to the tables list
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method to check if a restaurant table exists in the database.
        /// </summary>
        /// <param name="id">The ID to check for</param>
        /// <returns>True if table with given ID exists; false otherwise</returns>
        private bool RestaurantTableExists(int id)
        {
            return _context.RestaurantTables.Any(e => e.RestaurantTableId == id);
        }
    }
}
