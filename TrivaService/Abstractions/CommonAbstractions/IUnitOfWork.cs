using TrivaService.Abstractions.RepositoryAbstractions.ServiceRepositoryAbstractions;
using TrivaService.Abstractions.RepositoryAbstractions.StockRepositoryAbstractions;
using TrivaService.Abstractions.RepositoryAbstractions.UserRepositoryAbstractions;

namespace TrivaService.Abstractions.CommonAbstractions
{
    public interface IUnitOfWork
    {
        ICustomerRepository customerRepository { get; }
        IServiceRepository serviceRepository { get; }
        IServiceItemRepository serviceItemRepository { get; }
        IServiceVisualsRepository serviceVisualsRepository { get; }


        IItemRepository itemRepository { get; }
        ISupplierRepository supplierRepository { get; }

        IRolesRepository rolesRepository { get; }
        IUsersRepository usersRepository { get; }

        Task SaveAsync();
    }
}
