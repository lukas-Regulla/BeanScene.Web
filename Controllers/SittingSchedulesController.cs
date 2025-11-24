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
    public class SittingSchedulesController : Controller
    {
        private readonly BeanSceneContext _context;

        public SittingSchedulesController(BeanSceneContext context)
        {
            _context = context;
        }

        // GET: SittingSchedules
        public async Task<IActionResult> Index()
        {
            return View(await _context.SittingSchedules.ToListAsync());
        }

        // GET: SittingSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sittingSchedule = await _context.SittingSchedules
                .FirstOrDefaultAsync(m => m.SittingScheduleId == id);
            if (sittingSchedule == null)
            {
                return NotFound();
            }

            return View(sittingSchedule);
        }

        // GET: SittingSchedules/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SittingSchedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SittingScheduleId,Stype,StartDateTime,EndDateTime,Scapacity,Status,IsClosed")] SittingSchedule sittingSchedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sittingSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sittingSchedule);
        }

        // GET: SittingSchedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sittingSchedule = await _context.SittingSchedules.FindAsync(id);
            if (sittingSchedule == null)
            {
                return NotFound();
            }
            return View(sittingSchedule);
        }

        // POST: SittingSchedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SittingScheduleId,Stype,StartDateTime,EndDateTime,Scapacity,Status,IsClosed")] SittingSchedule sittingSchedule)
        {
            if (id != sittingSchedule.SittingScheduleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sittingSchedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SittingScheduleExists(sittingSchedule.SittingScheduleId))
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
            return View(sittingSchedule);
        }

        // GET: SittingSchedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sittingSchedule = await _context.SittingSchedules
                .FirstOrDefaultAsync(m => m.SittingScheduleId == id);
            if (sittingSchedule == null)
            {
                return NotFound();
            }

            return View(sittingSchedule);
        }

        // POST: SittingSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sittingSchedule = await _context.SittingSchedules.FindAsync(id);
            if (sittingSchedule != null)
            {
                _context.SittingSchedules.Remove(sittingSchedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SittingScheduleExists(int id)
        {
            return _context.SittingSchedules.Any(e => e.SittingScheduleId == id);
        }
    }
}
