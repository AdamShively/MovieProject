#nullable disable

using MovieProject.Models.Database;
using MovieProject.Models.TmDb;

namespace MovieProject.Services.Interfaces
{
    public interface IDataMappingService
    {
        Task<Movie> MapMovieDetailAsync(MovieDetail movie);
        ActorDetail MapActorDetail(ActorDetail actor);
    }
}
