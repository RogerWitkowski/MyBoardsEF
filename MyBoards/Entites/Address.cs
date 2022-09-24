using Newtonsoft.Json;

namespace MyBoards.Entites
{
    public class Address
    {
        public Guid Id { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public virtual User User { get; set; }
        public Guid UserId { get; set; }
        public Coordinates Coordinates { get; set; }
    }
}