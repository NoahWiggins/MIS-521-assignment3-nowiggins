using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIS_521_assignment3_nowiggins.Models
{
    public class ActorMovie
    {
        public int Id { get; set; }


        [ForeignKey("Actor")]
        public int? ActorId { get; set; }

        public Actor? Actor { get; set; }

        [ForeignKey("Movie")]

        public int? MovieId { get; set; }

        public Movie? Movie { get; set; }
    }
}
