#nullable disable

namespace MovieProject.Models.Settings
{
    public class MovieProjectSettings
    {
        public string DefaultBackdropSize { get; set; }
        public string DefaultPosterSize { get; set; }
        public string DefaultCastImage { get; set; }

        public DefaultCollection DefaultCollection { get; set; }
    }

    public class DefaultCollection
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}