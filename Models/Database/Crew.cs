#nullable disable

namespace MovieProject.Models.Database
{
    public class Crew
    {
        public int Id { get; set; }
        public int MovieId { get; set; }

        public int CrewID { get; set; }
        public string Department { get; set; }
        public string Name { get; set; }
        public string Job { get; set; }
        public string ImageUrl { get; set; }

        public Movie Movie { get; set; }
    }
}
