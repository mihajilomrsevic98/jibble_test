namespace Jibble.Interfaces
{
    using Jibble.Models;

    public interface IPeopleService
    {
        Task<IReadOnlyList<Person>> ListPeopleAsync(string filter = null);

        Task<Person> GetPersonDetailsAsync(string key);
    }
}
