using System.ComponentModel.DataAnnotations;

namespace MIS_521_assignment3_nowiggins.Models
{
    public class Actor
    {
        public int ActorId { get; set; }

        [Display(Name = "Name of Actor")]
        public string ActorName { get; set; }

        [Display(Name = "Gender")]

        public string ActorGender { get; set; }

        [Display(Name = "Age of Actor")]

        public string ActorAge { get; set; }

        [Display(Name = "IMDB Page ")]

        public string ActorIMDBHyperlink { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Actor  Picture")]
        public byte[]? ActorPhoto { get; set; }

        

    }
}
