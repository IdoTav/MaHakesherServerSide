
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaHakesherServerSide.Data;
using MaHakesherServerSide.Models;

namespace MaHakesherServerSide.Controllers
{
    public class RelationsController : Controller
    {
        private readonly MaHakesherServerSideContext _context;

        public RelationsController(MaHakesherServerSideContext context)
        {
            _context = context;
        }

        // GET: Relations
        public async Task<IActionResult> Index()
        {
              return _context.Relations != null ? 
                          View(await _context.Relations.ToListAsync()) :
                          Problem("Entity set 'MaHakesherServerSideContext.Relations'  is null.");
        }

        // GET: Relations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Relations == null)
            {
                return NotFound();
            }

            var relations = await _context.Relations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (relations == null)
            {
                return NotFound();
            }

            return View(relations);
        }

        // GET: Relations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Relations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Source,Destination")] Relations relations)
        {
            if (ModelState.IsValid)
            {
                _context.Add(relations);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(relations);
        }

        // GET: Relations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Relations == null)
            {
                return NotFound();
            }

            var relations = await _context.Relations.FindAsync(id);
            if (relations == null)
            {
                return NotFound();
            }
            return View(relations);
        }

        // POST: Relations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Source,Destination")] Relations relations)
        {
            if (id != relations.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(relations);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RelationsExists(relations.Id))
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
            return View(relations);
        }

        // GET: Relations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Relations == null)
            {
                return NotFound();
            }

            var relations = await _context.Relations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (relations == null)
            {
                return NotFound();
            }

            return View(relations);
        }

        // POST: Relations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Relations == null)
            {
                return Problem("Entity set 'MaHakesherServerSideContext.Relations'  is null.");
            }
            var relations = await _context.Relations.FindAsync(id);
            if (relations != null)
            {
                _context.Relations.Remove(relations);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RelationsExists(int id)
        {
          return (_context.Relations?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
