using Mystikweb.Auth.Demo.Models;

namespace Mystikweb.Auth.Demo;

public interface IAddressBookClient
{
    Task<IEnumerable<PersonItem>> GetPeopleAsync(CancellationToken cancellationToken = default);
}
