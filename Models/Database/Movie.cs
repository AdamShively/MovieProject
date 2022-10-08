#nullable disable

using MovieProject.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieProject.Models.Database
{
    public class Movie
    {
        public int Id { get; set; }
        public int TmBdMovieId { get; set; }
        public string Title { get; set; }
        public string TagLine { get; set; } 
        public string Overview { get; set; }
        public int RunTime { get; set; }

        [DataType(DataType.Date)]
        [Display(Name ="Release Date")]
        public DateTime ReleaseDate { get; set; }

        public Rating Rating { get; set; }
        public float VoteAvg { get; set; }

        [Display(Name = "Poster")]
        public byte[] PosterImageData { get; set; }
        public string PosterContentType { get; set; }
        [NotMapped]
        [Display(Name = "Poster Image")]
        public IFormFile PosterFile { get; set; }

        [Display(Name = "Backdrop")]
        public byte[] BackdropImageData { get; set; }
        public string BackdropContentType { get; set; }
        [NotMapped]
        [Display(Name = "Backdrop Image")]
        public IFormFile BackdropFile { get; set; }

        public string TrailerUrl { get; set; }

        public ICollection<MovieCollection> Collections { get; set; } = new HashSet<MovieCollection>();
        public ICollection<Cast> Cast { get; set; } = new HashSet<Cast>();
        public ICollection<Crew> Crew { get; set; } = new HashSet<Crew>();
    }
}
