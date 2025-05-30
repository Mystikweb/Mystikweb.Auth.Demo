using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Routing;

internal static class AddressBookEndpointBuilder
{
    internal static IEndpointConventionBuilder MapAddressBookEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/addressbook")
            .WithTags("Address Book")
            .RequireAuthorization();

        group.MapGet("/people", async ([FromServices] IAddressBookLogic logic, CancellationToken cancellationToken) =>
        {
            var people = await logic.GetPeopleAsync(cancellationToken);
            return Results.Ok(people);
        });

        return group;
    }
}
