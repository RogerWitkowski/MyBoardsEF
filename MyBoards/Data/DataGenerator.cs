using Bogus;
using MyBoards.Entites;

namespace MyBoards.Data
{
    public class DataGenerator
    {
        public static void Seed(MyBoardsDbContext dbContext)
        {
            var locale = "pl";

            Randomizer.Seed = new Random(1);

            var addressGenerator = new Faker<Address>(locale)
                //.StrictMode(true)
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.Country, f => f.Address.Country())
                .RuleFor(a => a.PostalCode, f => f.Address.ZipCode())
                .RuleFor(a => a.Street, f => f.Address.StreetName());

            // Address address = addressGenerator.Generate();

            var userGenerator = new Faker<User>(locale)
                .RuleFor(u => u.Email, f => f.Person.Email)
                .RuleFor(u => u.FullName, f => f.Person.FullName)
                //.RuleFor(u => u.Address, address);
                .RuleFor(u => u.Address, f => addressGenerator.Generate());

            var users = userGenerator.Generate(100);

            dbContext.AddRange(users);
            dbContext.SaveChanges();
        }
    }
}