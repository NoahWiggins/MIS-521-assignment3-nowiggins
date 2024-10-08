using System.ComponentModel.DataAnnotations;

namespace MIS_521_assignment3_nowiggins.Models
{
    public class Movie
    {
        public int MovieId { get; set; }

        [Required]

        [Display(Name = "Title")]

        public string MovieTitle { get; set; }


        [Display(Name = "Genre")]

        public string MovieGenre { get; set; }

        [Display(Name="Year of Release")]
        public DateOnly YearOfRelease { get; set; }

        [Display(Name = "Movie Hyperlink")]

        public string MovieIMDBHyperlink { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Movie Poster")]
        public byte[]? MoviePoster { get; set; }
        


    }
}
