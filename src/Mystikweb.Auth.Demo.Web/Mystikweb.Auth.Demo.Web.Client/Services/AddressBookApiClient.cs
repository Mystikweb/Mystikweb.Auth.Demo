using System.Net.Http.Json;

using Mystikweb.Auth.Demo.Models;

namespace Mystikweb.Auth.Demo.Web.Client.Services;

public sealed class AddressBookApiClient(HttpClient httpClient) : IAddressBookApiClient
{
    public async Task<IEnumerable<PersonItem>> GetPeopleAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<IEnumerable<PersonItem>>("api/addressbook/people", cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve address book items.");
}
