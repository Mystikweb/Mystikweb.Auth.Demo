namespace Mystikweb.Auth.Demo.ApiService.Services;

public sealed class AddressBookLogic(ApplicationDbContext context) : IAddressBookLogic
{
    public async Task<IEnumerable<PersonItem>> GetPeopleAsync(CancellationToken cancellationToken = default)
    {
        var people = await context.People.AsNoTracking().ToListAsync(cancellationToken);
        var addresses = await context.Addresses.AsNoTracking().ToListAsync(cancellationToken);

        return (from p in people
                let personAddresses = (from a in addresses
                                       where a.PersonId == p.Id
                                       select new PersonAddressItem
                                       {
                                           Id = a.Id,
                                           Line1 = a.Line1,
                                           Line2 = a.Line2,
                                           City = a.City,
                                           State = a.State,
                                           PostalCode = a.PostalCode,
                                           Country = a.Country,
                                           InsertBy = a.InsertBy,
                                           InsertAt = a.InsertAt.ToDateTimeOffset(),
                                           UpdateBy = a.UpdateBy,
                                           UpdateAt = a.UpdateAt.HasValue ? a.UpdateAt.Value.ToDateTimeOffset() : null
                                       }).ToList()
                select new PersonItem
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    BirthDate = p.BirthDate.HasValue ? p.BirthDate.Value.ToDateTimeUnspecified() : null,
                    Email = p.Email,
                    InsertBy = p.InsertBy,
                    InsertAt = p.InsertAt.ToDateTimeOffset(),
                    UpdateBy = p.UpdateBy,
                    UpdateAt = p.UpdateAt.HasValue ? p.UpdateAt.Value.ToDateTimeOffset() : null,
                    AddressItems = personAddresses
                }).ToList();
    }
}
