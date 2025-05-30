namespace Mystikweb.Auth.Demo.ApiService.Services;

public sealed class AddressBookLogic(ApplicationDbContext context) : IAddressBookLogic
{
    public async Task<IEnumerable<PersonItem>> GetPeopleAsync(CancellationToken cancellationToken = default)
    {
        return await (from p in context.People
                      let addresses = (from a in context.Addresses
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
                                           InsertAt = a.InsertAt,
                                           UpdateBy = a.UpdateBy,
                                           UpdateAt = a.UpdateAt
                                       }).ToList()
                        select new PersonItem
                        {
                            Id = p.Id,
                            FirstName = p.FirstName,
                            LastName = p.LastName,
                            BirthDate = p.BirthDate,
                            Email = p.Email,
                            InsertBy = p.InsertBy,
                            InsertAt = p.InsertAt,
                            UpdateBy = p.UpdateBy,
                            UpdateAt = p.UpdateAt,
                            AddressItems = addresses
                        }).AsNoTracking().ToListAsync(cancellationToken);
    }
}
