#nullable disable

namespace MovieProject.Models.Database
{
    public class Cast
    {
        public int Id { get; set; }
        public int MovieId { get; set; }

        public int CastID { get; set; }
        public string Department { get; set; }
        public string Name { get; set; }
        public string Character { get; set; }
        public string ImageUrl { get; set; }

        public Movie Movie { get; set; }

    }
}
