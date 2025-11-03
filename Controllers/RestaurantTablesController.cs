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
    [Authorize(Roles = "Admin,Staff")]
    public class RestaurantTablesController : Controller
    {
        private readonly BeanSceneContext _context;

        public RestaurantTablesController(BeanSceneContext context)
        {
            _context = context;
        }

        // GET: RestaurantTables
        public async Task<IActionResult> Index()
        {
            var beanSceneContext = _context.RestaurantTables.Include(r => r.Area);
            return View(await beanSceneContext.ToListAsync());
        }

        // GET: RestaurantTables/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurantTable = await _context.RestaurantTables
                .Include(r => r.Area)
                .FirstOrDefaultAsync(m => m.RestaurantTableId == id);
            if (restaurantTable == null)
            {
                return NotFound();
            }

            return View(restaurantTable);
        }

        // GET: RestaurantTables/Create
        public IActionResult Create()
        {
            ViewData["AreaId"] = new SelectList(_context.Areas, "AreaId", "AreaId");
            return View();
        }

        // POST: RestaurantTables/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RestaurantTableId,AreaId,TableName,Seats")] RestaurantTable restaurantTable)
        {
            if (ModelState.IsValid)
            {
                _context.Add(restaurantTable);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AreaId"] = new SelectList(_context.Areas, "AreaId", "AreaId", restaurantTable.AreaId);
            return View(restaurantTable);
        }

        // GET: RestaurantTables/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurantTable = await _context.RestaurantTables.FindAsync(id);
            if (restaurantTable == null)
            {
                return NotFound();
            }
            ViewData["AreaId"] = new SelectList(_context.Areas, "AreaId", "AreaId", restaurantTable.AreaId);
            return View(restaurantTable);
        }

        // POST: RestaurantTables/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RestaurantTableId,AreaId,TableName,Seats")] RestaurantTable restaurantTable)
        {
            if (id != restaurantTable.RestaurantTableId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(restaurantTable);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RestaurantTableExists(restaurantTable.RestaurantTableId))
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
            ViewData["AreaId"] = new SelectList(_context.Areas, "AreaId", "AreaId", restaurantTable.AreaId);
            return View(restaurantTable);
        }

        // GET: RestaurantTables/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurantTable = await _context.RestaurantTables
                .Include(r => r.Area)
                .FirstOrDefaultAsync(m => m.RestaurantTableId == id);
            if (restaurantTable == null)
            {
                return NotFound();
            }

            return View(restaurantTable);
        }

        // POST: RestaurantTables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var restaurantTable = await _context.RestaurantTables.FindAsync(id);
            if (restaurantTable != null)
            {
                _context.RestaurantTables.Remove(restaurantTable);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RestaurantTableExists(int id)
        {
            return _context.RestaurantTables.Any(e => e.RestaurantTableId == id);
        }
    }
}
