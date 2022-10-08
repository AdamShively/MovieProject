#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieProject.Data;
using MovieProject.Models.Database;
using MovieProject.Models.Settings;
using System.Net;
using System.Reflection.Metadata;

namespace MovieProject.Controllers
{
    public class MovieCollectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;
        public MovieCollectionsController(ApplicationDbContext context, IOptions<AppSettings> appSettings, UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _context = context;
            _appSettings = appSettings.Value;
            _userManager = userManager;
            _config = config;
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (!User.IsInRole("Administrator"))
            {
                return View("AccessDenied");
            }

            var defaultCollectionName = _appSettings.MovieProjectSettings.DefaultCollection.Name;
            id ??= (await _context.Collection.FirstOrDefaultAsync(c => c.Name.ToUpper() == defaultCollectionName.ToUpper())).Id;

            var collection = await _context.Collection.FirstOrDefaultAsync(c => c.Id == id);

            //user presented w/ name of collection, id given to post
            ViewData["CollectionId"] = new SelectList(_context.Collection, "Id", "Name", id);

            //list of ids of movies in system
            var allMovieIds = await _context.Movie.Select(m => m.Id).ToListAsync();

            var movieIdsInCollection = await _context.MovieCollection
                                        .Where(m => m.CollectionId == id)
                                        .OrderBy(m => m.Order)
                                        .Select(m => m.MovieId)
                                        .ToListAsync();

            var movieIdsNotInCollection = allMovieIds.Except(movieIdsInCollection);

            var moviesInCollection = new List<Movie>();
            movieIdsInCollection.ForEach(movieId => moviesInCollection.Add(_context.Movie.Find(movieId)));

            ViewData["IdsInCollection"] = new MultiSelectList(moviesInCollection, "Id", "Title");

            var moviesNotInCollection = await _context.Movie.AsNoTracking().Where(m => movieIdsNotInCollection.Contains(m.Id)).ToListAsync();
            ViewData["IdsNotInCollection"] = new MultiSelectList(moviesNotInCollection, "Id", "Title");

            ViewData["api_key"] = _config["TmDbApiKey"];

            ViewData["Title"] = "Movie Collections";

            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int id, List<int> idsInCollection)
        {
            var oldRecords = _context.MovieCollection.Where(c => c.CollectionId == id);
            _context.MovieCollection.RemoveRange(oldRecords);
            await _context.SaveChangesAsync();

            if (idsInCollection != null)
            {
                int index = 1;
                idsInCollection.ForEach(movieId =>
                {
                    _context.Add(new MovieCollection()
                    {
                        CollectionId = id,
                        MovieId = movieId,
                        Order = index++
                    });
                });

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { id });
        }
    }
}