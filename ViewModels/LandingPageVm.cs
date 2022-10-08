#nullable disable
using MovieProject.Models.Database;
using MovieProject.Models.TmDb;

namespace MovieProject.ViewModels
{
    public class LandingPageVm
    {
        public List<Collection> CustomCollections { get; set; }
        public MovieSearch NowPlaying { get; set; }
        public MovieSearch Popular { get; set; }
        public MovieSearch TopRated { get; set; }
        public MovieSearch Upcoming { get; set; }
    }
}
