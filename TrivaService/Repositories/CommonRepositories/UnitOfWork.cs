using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Abstractions.RepositoryAbstractions.ServiceRepositoryAbstractions;
using TrivaService.Abstractions.RepositoryAbstractions.StockRepositoryAbstractions;
using TrivaService.Abstractions.RepositoryAbstractions.UserRepositoryAbstractions;
using TrivaService.Data;

namespace TrivaService.Repositories.CommonRepositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;

        public UnitOfWork(
            AppDbContext dbContext,
            ICustomerRepository customerRepository,
            IServiceRepository serviceRepository,
            IServiceItemRepository serviceItemRepository,
            IServiceVisualsRepository serviceVisualsRepository,
            IItemRepository itemRepository,
            ISupplierRepository supplierRepository,
            IRolesRepository rolesRepository,
            IUsersRepository usersRepository)
        {
            _dbContext = dbContext;
            this.customerRepository = customerRepository;
            this.serviceRepository = serviceRepository;
            this.serviceItemRepository = serviceItemRepository;
            this.serviceVisualsRepository = serviceVisualsRepository;
            this.itemRepository = itemRepository;
            this.supplierRepository = supplierRepository;
            this.rolesRepository = rolesRepository;
            this.usersRepository = usersRepository;
        }

        public ICustomerRepository customerRepository { get; }
        public IServiceRepository serviceRepository { get; }
        public IServiceItemRepository serviceItemRepository { get; }
        public IServiceVisualsRepository serviceVisualsRepository { get; }
        public IItemRepository itemRepository { get; }
        public ISupplierRepository supplierRepository { get; }
        public IRolesRepository rolesRepository { get; }
        public IUsersRepository usersRepository { get; }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
