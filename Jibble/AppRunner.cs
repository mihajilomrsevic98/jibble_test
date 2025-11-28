namespace Jibble
{
    using Jibble.Interfaces;

    public class AppRunner
    {
        private readonly IPeopleService _peopleService;

        public AppRunner(IPeopleService peopleService)
        {
            this._peopleService = peopleService;
        }

        public async Task RunAsync()
        {
            bool running = true;
            while (running)
            {
                this.DisplayMenu();
                var input = Console.ReadLine()?.Trim().ToUpper();

                try
                {
                    switch (input)
                    {
                        case "1": await this.ListPeopleAsync(null); break;
                        case "2": await this.SearchPeopleAsync(); break;
                        case "3": await this.ShowDetailsAsync(); break;
                        case "4": running = false; break;
                        default: Console.WriteLine("Invalid option. Please try again."); break;
                    }
                }
                catch (ODataServiceException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n[Service Error] {ex.Message}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"\n[Application Error] An unexpected error occurred: {ex.Message}");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\nExiting application. Goodbye!");
        }

        private void DisplayMenu()
        {
            Console.WriteLine("\n====================================");
            Console.WriteLine("  OData People Client (v4)");
            Console.WriteLine("====================================");
            Console.WriteLine("1. List All People");
            Console.WriteLine("2. Search/Filter People (OData $filter)");
            Console.WriteLine("3. Show Details on a specific Person (by Username)");
            Console.WriteLine("4. Exit");
            Console.Write("Enter option: ");
        }

        private async Task ListPeopleAsync(string filter)
        {
            Console.WriteLine($"\n--- Listing People (Filter: {filter ?? "None"}) ---");
            var people = await this._peopleService.ListPeopleAsync(filter);

            if (people.Any())
            {
                foreach (var person in people)
                {
                    Console.WriteLine($"{person.UserName}\t\t\t{person.FirstName}\t\t{person.LastName}");
                }

                Console.WriteLine($"\nTotal Results: {people.Count}");
            }
            else
            {
                Console.WriteLine("No people found matching the criteria.");
            }
        }

        private async Task SearchPeopleAsync()
        {
            Console.Write("\nEnter OData $filter string (e.g., FirstName eq 'Scott' or LastName eq 'Jones'): ");
            var filter = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(filter))
            {
                Console.WriteLine("Search cancelled.");
                return;
            }

            await this.ListPeopleAsync(filter);
        }

        private async Task ShowDetailsAsync()
        {
            Console.Write("\nEnter Person Username (e.g., 'scott'): ");
            var key = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(key))
            {
                Console.WriteLine("Operation cancelled.");
                return;
            }

            var person = await this._peopleService.GetPersonDetailsAsync(key);

            Console.WriteLine("\n--- Person Details ---");
            Console.WriteLine($"  Username: {person.UserName}");
            Console.WriteLine($"  Full Name: {person.FirstName} {person.LastName}");
            Console.WriteLine($"  Emails: {string.Join(", ", person.Emails)}");
            Console.WriteLine($"  Age: {person.Age?.ToString() ?? "NOT FOUND"}");
            Console.WriteLine($"  FavoriteFeature: {string.Join(", ", person.FavoriteFeature)}");
            Console.WriteLine("----------------------");
        }
    }
}
