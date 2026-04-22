using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace TrivaService.Data
{
    public class DapperContext
    {
        private readonly DapperOptions _dapperOptions;

        public DapperContext(IOptions<DapperOptions> dapperOptions)
        {
            _dapperOptions = dapperOptions.Value;
        }

        public int CommandTimeoutSeconds => _dapperOptions.CommandTimeoutSeconds;

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_dapperOptions.ConnectionString);
        }
    }
}
