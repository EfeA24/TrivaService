using Dapper;
using Microsoft.EntityFrameworkCore;
using TrivaService.Abstractions.CommonAbstractions;
using TrivaService.Data;
using TrivaService.Models.TechnicalEntities;

namespace TrivaService.Repositories.CommonRepositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly AppDbContext _dbContext;
        private readonly DapperContext _dapperContext;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext dbContext, DapperContext dapperContext)
        {
            _dbContext = dbContext;
            _dapperContext = dapperContext;
            _dbSet = dbContext.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var tableName = GetTableName();
            using var connection = _dapperContext.CreateConnection();
            return await connection.QueryAsync<T>($"SELECT * FROM [{tableName}]");
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            var tableName = GetTableName();
            using var connection = _dapperContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<T>(
                $"SELECT * FROM [{tableName}] WHERE [Id] = @Id",
                new { Id = id });
        }

        public async Task CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
            if (entity is not null)
            {
                _dbSet.Remove(entity);
            }
        }

        public async Task<int> CountAsync()
        {
            var tableName = GetTableName();
            using var connection = _dapperContext.CreateConnection();
            return await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM [{tableName}]");
        }

        private string GetTableName()
        {
            return _dbContext.Model.FindEntityType(typeof(T))?.GetTableName() ?? typeof(T).Name;
        }
    }
}
