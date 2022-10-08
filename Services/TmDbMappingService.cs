#nullable disable

using Microsoft.Extensions.Options;
using MovieProject.Enums;
using MovieProject.Models.Database;
using MovieProject.Models.Settings;
using MovieProject.Models.TmDb;
using MovieProject.Services.Interfaces;

namespace MovieProject.Services
{
    public class TmDbMappingService : IDataMappingService
    {
        private readonly AppSettings _appSettings;
        private readonly IImageService _imageService;

        public TmDbMappingService(IOptions<AppSettings> appSettings, IImageService imageService)
        {
            _appSettings = appSettings.Value;
            _imageService = imageService;
        }

        public async Task<Movie> MapMovieDetailAsync(MovieDetail movie)
        {
            Movie newMovie = null;

            try
            {
                newMovie = new Movie()
                {
                    TmBdMovieId = movie.id,
                    Title = movie.title,
                    TagLine = movie.tagline,
                    Overview = movie.overview,
                    RunTime = movie.runtime,
                    BackdropImageData = await EncodeBackdropImageAsync(movie.backdrop_path),
                    BackdropContentType = BuildImageType(movie.backdrop_path),
                    PosterImageData = await EncodePosterImageAsync(movie.poster_path),
                    PosterContentType = BuildImageType(movie.poster_path),
                    Rating = GetRating(movie.release_dates),
                    ReleaseDate = DateTime.Parse(movie.release_date).ToUniversalTime(),
                    TrailerUrl = BuildTrailerPath(movie.videos),
                    VoteAvg = movie.vote_average

                };

                var castMembers = movie.credits.cast.OrderByDescending(c => c.popularity)
                                                    .GroupBy(c => c.cast_id)
                                                    .Select(g => g.FirstOrDefault())
                                                    .Take(20).ToList();

                castMembers.ForEach(member =>
                {
                    newMovie.Cast.Add(new Models.Database.Cast()
                    {
                        CastID = member.id,
                        Department = member.known_for_department,
                        Name = member.name,
                        Character = member.character,
                        ImageUrl = BuildCastImage(member.profile_path),
                    });
                });

                var crewMembers = movie.credits.crew.OrderByDescending(c => c.popularity)
                                                    .GroupBy(c => c.id)
                                                    .Select(g => g.First())
                                                    .Take(20).ToList();

                crewMembers.ForEach(member =>
                {
                    newMovie.Crew.Add(new Models.Database.Crew()
                    {
                        CrewID = member.id,
                        Department = member.department,
                        Name = member.name,
                        Job = member.job,
                        ImageUrl = BuildCastImage(member.profile_path)
                    });
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in MapMovieDetailAsync: {ex.Message}");

            }

            return newMovie;
        }

        public ActorDetail MapActorDetail(ActorDetail actor)
        {
            actor.profile_path = BuildCastImage(actor.profile_path);

            if (string.IsNullOrEmpty(actor.biography))
            {
                actor.biography = "Not Available";
            }

            if (string.IsNullOrEmpty(actor.place_of_birth))
            {
                actor.place_of_birth = "Not Available";
            }

            if (string.IsNullOrEmpty(actor.birthday))
                actor.birthday = "Not Available";
            else
                actor.birthday = DateTime.Parse(actor.birthday).ToString("MMM dd, yyyy");

            return actor;
        }



        private async Task<byte[]> EncodeBackdropImageAsync(string path)
        {
            var backdropPath = $"{_appSettings.TmDbSettings.BaseImagePath}/{_appSettings.MovieProjectSettings.DefaultBackdropSize}/{path}";
            return await _imageService.EncodeImageURLAsync(backdropPath);
        }

        private string BuildImageType(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
                
            return $"image/{Path.GetExtension(path).TrimStart('.')}";
        }

        private async Task<byte[]> EncodePosterImageAsync(string path)
        {
            var posterPath = $"{_appSettings.TmDbSettings.BaseImagePath}/{_appSettings.MovieProjectSettings.DefaultPosterSize}/{path}";
            return await _imageService.EncodeImageURLAsync(posterPath);
        }

        private Rating GetRating(Release_Dates dates)
        {
            var movieRating = Rating.NR;

            if (dates != null)
            {
                var certification = dates.results.FirstOrDefault(r => r.iso_3166_1 == "US");
                if (certification is not null)
                {
                    var apiRating = certification.release_dates.FirstOrDefault(c => c.certification != "")?.certification.Replace("-", "");
                    if (!string.IsNullOrEmpty(apiRating))
                    {
                        movieRating = (Rating)Enum.Parse(typeof(Rating), apiRating, true);
                    }
                }
            }
            return movieRating;
        }

        private string BuildTrailerPath(Videos videos)
        {
            var videoKey = videos.results.FirstOrDefault(r => r.type.ToLower().Trim() == "trailer" && r.key != "")?.key;
            return string.IsNullOrEmpty(videoKey) ? videoKey : $"{_appSettings.TmDbSettings.BaseYouTubePath}{videoKey}";
        }

        private string BuildCastImage(string profilePath)
        {
            if (string.IsNullOrEmpty(profilePath))
                return _appSettings.MovieProjectSettings.DefaultCastImage;

            return $"{_appSettings.TmDbSettings.BaseImagePath}/{_appSettings.MovieProjectSettings.DefaultPosterSize}/{profilePath}";
        }

    }
}