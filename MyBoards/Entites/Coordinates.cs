using Microsoft.EntityFrameworkCore;

namespace MyBoards.Entites
{
    //[Owned]
    public class Coordinates
    {
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
    }
}