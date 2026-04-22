using TrivaService.Abstractions.RepositoryAbstractions.StockRepositoryAbstractions;
using TrivaService.Data;
using TrivaService.Models.StockEntities;
using TrivaService.Repositories.CommonRepositories;

namespace TrivaService.Repositories.RepositoryImplementations.StockRepositoryImplementations
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(AppDbContext dbContext, DapperContext dapperContext) : base(dbContext, dapperContext)
        {
        }
    }
}
