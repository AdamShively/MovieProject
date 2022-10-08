using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MovieProject.Models.Database;
using MovieProject.Models.Settings;
using MovieProject.Services.Interfaces;

namespace MovieProject.Controllers
{
    public class ActorController : Controller
    {
        private readonly IRemoteMovieService _tmdbMovieService;
        private readonly IDataMappingService _mappingService;
        private readonly IConfiguration _config;

        public ActorController(IRemoteMovieService tmdbMovieService, IDataMappingService mappingService, IConfiguration config)
        {
            _tmdbMovieService = tmdbMovieService;
            _mappingService = mappingService;
            _config = config;
        }

        public async Task<IActionResult> Detail(int id)
        {
            var actor = await _tmdbMovieService.ActorDetailAsync(id);

            actor = _mappingService.MapActorDetail(actor);

            ViewData["Title"] = actor.name;
            ViewData["api_key"] = _config["TmDbApiKey"];
            return View(actor);
        }
    }
}