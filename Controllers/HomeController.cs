#nullable disable

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieProject.Data;
using MovieProject.Enums;
using MovieProject.Models;
using MovieProject.ViewModels;
using MovieProject.Models.Settings;
using MovieProject.Services.Interfaces;
using System.Diagnostics;
using X.PagedList;
using MovieProject.Services;
using Microsoft.AspNetCore.Identity;
using MailKit.Net.Smtp;

namespace MovieProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRemoteMovieService _tmdbMovieService;
        private readonly IMovieEmailSender _emailSender;
        private readonly IConfiguration _config;

        public HomeController(ApplicationDbContext context, IRemoteMovieService tmdbMovieService, IMovieEmailSender emailSender, IConfiguration config)
        {
            _context = context;
            _tmdbMovieService = tmdbMovieService;
            _emailSender = emailSender;
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            const int count = 20;
            var data = new LandingPageVm()
            {
                CustomCollections = await _context.Collection
                                            .Include(c => c.MovieCollections)
                                            .ThenInclude(mc => mc.Movie)
                                            .ToListAsync(),
                NowPlaying = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.now_playing, count),
                Popular = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.popular, count),
                TopRated = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.top_rated, count),
                Upcoming = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.upcoming, count)
            };
            ViewData["Title"] = "Homepage";
            ViewData["api_key"] = _config["TmDbApiKey"];
            return View(data);
        }
        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact Me";
            ViewData["api_key"] = _config["TmDbApiKey"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMe model)
        {
            try
            {
                model.Message = $"{model.Message} <hr/> Phone: {model.PhoneNumber}";
                await _emailSender.SendContactEmailAsync(model.Email, model.Name, model.Subject, model.Message);

                TempData["displayMsg"] = "Message sent successfully.";
                return RedirectToAction("Contact");
            }
            catch (SmtpCommandException)
            {
                TempData["displayMsg"] = "Daily user sending quota exceeded. Please try again later or send an email to adammshively89@gmail.com";
                return RedirectToAction("Contact");
            }
        }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}