#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieProject.Data;
using MovieProject.Models.Database;
using MovieProject.Models.Settings;
using MovieProject.Models.TmDb;
using MovieProject.Services.Interfaces;

namespace MovieProject.Controllers
{
    public class MoviesController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly ApplicationDbContext _context;
        private readonly IRemoteMovieService _tmdbMovieService;
        private readonly IDataMappingService _tmdbMappingService;
        private readonly IConfiguration _config;

        public MoviesController(IOptions<AppSettings> appSettings, ApplicationDbContext context, IRemoteMovieService tmdbMovieService, IDataMappingService tmdbMappingService, IConfiguration config)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _tmdbMovieService = tmdbMovieService;
            _tmdbMappingService = tmdbMappingService;
            _config = config;
        }

        public async Task<IActionResult> Details(int? id, bool local = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            Movie movie = new();
            if (local)
            {
                //Get the Movie data straight from the DB
                movie = await _context.Movie.Include(m => m.Cast)
                                            .Include(m => m.Crew)
                                            .FirstOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                //Get the movie data from the TMDB API
                var movieDetail = await _tmdbMovieService.MovieDetailAsync((int)id);
                movie = await _tmdbMappingService.MapMovieDetailAsync(movieDetail);
            }

            if (movie == null)
            {
                return NotFound();
            }

            ViewData["Title"] = movie.Title;
            ViewData["Local"] = local;
            ViewData["api_key"] = _config["TmDbApiKey"];
            return View(movie);

        }

        // POST: Temp/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FirstOrDefaultAsync(m => m.TmBdMovieId == id);
            _context.Movie.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction("Library", "Movies");
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(int id)
        { 
            //If we already have this movie we can just warn the user instead of importing it again
            if (_context.Movie.Any(m => m.TmBdMovieId == id))
            {
                TempData["errorMsg"] = "Movie already imported.";
                var localMovie = await _context.Movie.FirstOrDefaultAsync(m => m.TmBdMovieId == id);
                return RedirectToAction("Index", "MovieCollections");
            }

            //Step 1: Get the raw data from the API
            var movieDetail = await _tmdbMovieService.MovieDetailAsync(id);

            //Step 2: Run the data through a mapping procedure
            var movie = await _tmdbMappingService.MapMovieDetailAsync(movieDetail);

            if (movie == null)
            {
                TempData["errorMsg"] = "Import unsuccessful.";
                return RedirectToAction("Index", "MovieCollections");
            }
            //Step 3: Add the new movie
            _context.Add(movie);
            await _context.SaveChangesAsync();

            //Step 4: Assign it to the default All Collection
            await AddToMovieCollection(movie.Id, _appSettings.MovieProjectSettings.DefaultCollection.Name);

            TempData["displayMsg"] = "Movie successfully imported.";
            return RedirectToAction("Index", "MovieCollections");
        }

        public async Task<IActionResult> Library()
        {
            var movies = await _context.Movie.ToListAsync();

            ViewData["Title"] = "Library";
            ViewData["api_key"] = _config["TmDbApiKey"];

            return View(movies);
        }

        private async Task AddToMovieCollection(int movieId, string collectionName)
        {
            var collection = await _context.Collection.FirstOrDefaultAsync(c => c.Name == collectionName);
            _context.Add(
                new MovieCollection()
                {
                    CollectionId = collection.Id,
                    MovieId = movieId
                }
            );
            await _context.SaveChangesAsync();
        }

        private async Task AddToMovieCollection(int movieId, int collectionId)
        {
            _context.Add(
                new MovieCollection()
                {
                    CollectionId = collectionId,
                    MovieId = movieId
                }
            );
            await _context.SaveChangesAsync();
        }
    }
}
