using TrivaService.Abstractions.RepositoryAbstractions.ServiceRepositoryAbstractions;
using TrivaService.Data;
using TrivaService.Models.ServiceEntites;
using TrivaService.Repositories.CommonRepositories;

namespace TrivaService.Repositories.RepositoryImplementations.ServiceRepositoryImplementations
{
    public class ServiceRepository : GenericRepository<Service>, IServiceRepository
    {
        public ServiceRepository(AppDbContext dbContext, DapperContext dapperContext) : base(dbContext, dapperContext)
        {
        }
    }
}
