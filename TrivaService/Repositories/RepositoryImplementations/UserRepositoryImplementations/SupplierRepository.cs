using TrivaService.Abstractions.RepositoryAbstractions.UserRepositoryAbstractions;
using TrivaService.Data;
using TrivaService.Models.UserEntities;
using TrivaService.Repositories.CommonRepositories;

namespace TrivaService.Repositories.RepositoryImplementations.UserRepositoryImplementations
{
    public class RolesRepository : GenericRepository<Roles>, IRolesRepository
    {
        public RolesRepository(AppDbContext dbContext, DapperContext dapperContext) : base(dbContext, dapperContext)
        {
        }
    }
}
