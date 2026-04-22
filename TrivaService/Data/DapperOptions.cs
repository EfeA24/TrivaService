namespace TrivaService.Data
{
    public class DapperOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public int CommandTimeoutSeconds { get; set; } = 30;
    }
}
