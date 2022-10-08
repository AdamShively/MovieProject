#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieProject.Data;
using MovieProject.Enums;
using MovieProject.Models.Database;
using MovieProject.Models.Settings;

namespace MovieProject.Controllers
{
    public class CollectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AppSettings _appSettings;

        public CollectionsController(ApplicationDbContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }

        // GET: Collections
        public async Task<IActionResult> Index()
        {
            var defaultCollectionName = _appSettings.MovieProjectSettings.DefaultCollection.Name;
            var collections = await _context.Collection.Where(c => c.Name != defaultCollectionName).ToListAsync();

            return View(collections);
        }

        // POST: Collections/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Collection collection)
        {
            if (collection.Name == _appSettings.MovieProjectSettings.DefaultCollection.Name)
            {
                TempData["displayMsg"] = $"Creating a collection named \"{collection.Name}\" is forbidden.";
                return RedirectToAction("Index", "MovieCollections", collection.Id);
            }
            _context.Add(collection);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "MovieCollections", new { id = collection.Id });
        }

        // POST: Collections/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Collection collection)
        {
            if (id != collection.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (collection.Name == _appSettings.MovieProjectSettings.DefaultCollection.Name)
                    {
                        TempData["displayMsg"] = $"Changing a collection named \"{collection.Name}\" is forbidden.";
                        return RedirectToAction("Index", "MovieCollections", collection.Id);
                    }

                    _context.Update(collection);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CollectionExists(collection.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "MovieCollections");
            }
            return RedirectToAction("Index", "MovieCollections");
        }

        // POST: Collections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var collection = await _context.Collection.FindAsync(id);

            if (collection.Name == _appSettings.MovieProjectSettings.DefaultCollection.Name)
            {
                TempData["displayMsg"] = $"Deleting the collection named \"{collection.Name}\" is forbidden.";
                return RedirectToAction("Index", "MovieCollections", collection.Id);
            }
            _context.Collection.Remove(collection);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "MovieCollections");
        }

        private bool CollectionExists(int id)
        {
            return _context.Collection.Any(e => e.Id == id);
        }
    }
}