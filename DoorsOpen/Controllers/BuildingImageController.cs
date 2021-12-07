using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoorsOpen.Data;
using DoorsOpen.Models;

namespace DoorsOpen.Controllers
{
    public class BuildingImageController : Controller
    {
        private readonly SiteDbContext _context;

        public BuildingImageController(SiteDbContext context)
        {
            _context = context;
        }

        // GET: BuildingImage
        public async Task<IActionResult> Index()
        {
            return View(await _context.BuildingImages.ToListAsync());
        }

        // GET: BuildingImage/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buildingImageModel = await _context.BuildingImages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (buildingImageModel == null)
            {
                return NotFound();
            }

            return View(buildingImageModel);
        }

        // GET: BuildingImage/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.allBuildingModels = await  _context.Buildings.ToListAsync();

            return View();
        }

        // POST: BuildingImage/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,BuildingId,ImageURL,AltText")] BuildingImageModel buildingImageModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(buildingImageModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(buildingImageModel);
        }

        // GET: BuildingImage/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buildingImageModel = await _context.BuildingImages.FindAsync(id);
            if (buildingImageModel == null)
            {
                return NotFound();
            }
            return View(buildingImageModel);
        }

        // POST: BuildingImage/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BuildingId,ImageURL,AltText")] BuildingImageModel buildingImageModel)
        {
            if (id != buildingImageModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(buildingImageModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BuildingImageModelExists(buildingImageModel.Id))
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
            return View(buildingImageModel);
        }

        // GET: BuildingImage/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buildingImageModel = await _context.BuildingImages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (buildingImageModel == null)
            {
                return NotFound();
            }

            return View(buildingImageModel);
        }

        // POST: BuildingImage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var buildingImageModel = await _context.BuildingImages.FindAsync(id);
            _context.BuildingImages.Remove(buildingImageModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BuildingImageModelExists(int id)
        {
            return _context.BuildingImages.Any(e => e.Id == id);
        }
    }
}
