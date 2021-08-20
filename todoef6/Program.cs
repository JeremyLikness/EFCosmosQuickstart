using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace todo
{
    class Program
    {
        private const string EndpointUrl = "https://localhost:8081/";
        private const string AuthorizationKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string DatabaseId = "FamilyDatabase-EF6";

        private static readonly DbContextOptions<FamilyContext>
            Options = new DbContextOptionsBuilder<FamilyContext>()
            .UseCosmos(EndpointUrl, AuthorizationKey, DatabaseId)
            .Options;

        private static FamilyContext GetContext() =>
        new FamilyContext(Options);

        static async Task Main(string[] args)
        {
            await Program.CreateDatabaseAsync();
            await Program.AddItemsToContainerAsync();
            await Program.QueryItemsAsync();
            await Program.ReplaceFamilyItemAsync();
            await Program.DeleteFamilyItemAsync();
            await Program.DeleteDatabaseAndCleanupAsync();
        }

        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private static async Task CreateDatabaseAsync()
        {
            // Create a new database
            using var context = GetContext();
            await context.Database.EnsureCreatedAsync();
        }

        /// <summary>
        /// Add Family items to the container
        /// </summary>
        private static async Task AddItemsToContainerAsync()
        {
            // Create a family object for the Andersen family
            Family andersenFamily = new Family
            {
                Id = "Andersen.1",
                LastName = "Andersen",
                Parents = new List<Parent>()
                {
            new Parent { FirstName = "Thomas" },
            new Parent { FirstName = "Mary Kay" }
                },
                Children = new List<Child>()
                {
            new Child
            {
                FirstName = "Henriette Thaulow",
                Gender = "female",
                Grade = 5,
                Pets = new List<Pet>()
                {
                    new Pet { GivenName = "Fluffy" }
                }
            }
                },
                Address = new Address { State = "WA", County = "King", City = "Seattle" },
                IsRegistered = false
            };

            using var context = GetContext();

            try
            {
                var family = await context.Families.SingleAsync(f => f.Id == andersenFamily.Id);
                Console.WriteLine("Item in database with id: {0} already exists\n", family.Id);
            }
            catch
            {
                context.Add(andersenFamily);
                await context.SaveChangesAsync();
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse.
                Console.WriteLine("Created item in database with id: {0}\n", andersenFamily.Id);
            }

            // Create a family object for the Wakefield family
            Family wakefieldFamily = new Family
            {
                Id = "Wakefield.7",
                LastName = "Wakefield",
                Parents = new List<Parent>()
                {
            new Parent { FamilyName = "Wakefield", FirstName = "Robin" },
            new Parent { FamilyName = "Miller", FirstName = "Ben" }
                },
                Children = new List<Child>()
                {
            new Child
            {
                FamilyName = "Merriam",
                FirstName = "Jesse",
                Gender = "female",
                Grade = 8,
                Pets = new List<Pet>()
                {
                    new Pet { GivenName = "Goofy" },
                    new Pet { GivenName = "Shadow" }
                }
            },
            new Child
            {
                FamilyName = "Miller",
                FirstName = "Lisa",
                Gender = "female",
                Grade = 1
            }
                },
                Address = new Address { State = "NY", County = "Manhattan", City = "NY" },
                IsRegistered = true
            };

            context.Add(wakefieldFamily);
            await context.SaveChangesAsync();

            Console.WriteLine("Created item in database with id: {0}\n", wakefieldFamily.Id);
        }

        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// </summary>
        private static async Task QueryItemsAsync()
        {
            using var context = GetContext();
            var familyQuery = context.Families.Where(f => f.LastName == "Andersen");

            Console.WriteLine("Running query: {0}\n", familyQuery.ToQueryString());

            var families = await familyQuery.ToListAsync();

            foreach (Family family in families)
            {
                Console.WriteLine("\tRead {0}\n", family);
            }
        }

        /// <summary>
        /// Replace an item in the container
        /// </summary>
        private static async Task ReplaceFamilyItemAsync()
        {
            using var context = GetContext();
            var itemBody = await context.Families.WithPartitionKey("Wakefield").SingleAsync(f => f.Id == "Wakefield.7");

            // update registration status from false to true
            itemBody.IsRegistered = true;

            // update grade of child
            itemBody.Children[0].Grade = 6;

            await context.SaveChangesAsync();
            Console.WriteLine("Updated Family [{0},{1}].\n \tBody is now: {2}\n", itemBody.LastName, itemBody.Id, itemBody);
        }

        /// <summary>
        /// Delete an item in the container
        /// </summary>
        private static async Task DeleteFamilyItemAsync()
        {
            using var context = GetContext();

            var familyToDelete = new Family { Id = "Wakefield.7", LastName = "Wakefield" };
            context.Entry(familyToDelete).State = EntityState.Deleted;
            await context.SaveChangesAsync();

            Console.WriteLine("Deleted Family [{0},{1}]\n", familyToDelete.LastName, familyToDelete.Id);
        }

        /// <summary>
        /// Delete the database and dispose of the Cosmos Client instance
        /// </summary>
        private static async Task DeleteDatabaseAndCleanupAsync()
        {
            using var context = GetContext();
            await context.Database.EnsureDeletedAsync();
            Console.WriteLine("Deleted Database: {0}\n", Program.DatabaseId);
        }
    }
}
