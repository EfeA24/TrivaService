using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TrivaService.Data;

/// <summary>
/// Ensures <c>dotnet ef</c> loads appsettings from the project directory even when the shell cwd differs.
/// </summary>
public sealed class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var baseDir = AppContext.BaseDirectory;
        var projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));

        // #region agent log
        try
        {
            var logPath = Path.GetFullPath(Path.Combine(projectDir, "..", "debug-dcd861.log"));
            var line = JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["sessionId"] = "dcd861",
                ["hypothesisId"] = "H3",
                ["location"] = "AppDbContextDesignTimeFactory.CreateDbContext",
                ["message"] = "design_time_paths",
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ["data"] = new Dictionary<string, object?>
                {
                    ["baseDir"] = baseDir,
                    ["projectDir"] = projectDir,
                    ["cwd"] = Directory.GetCurrentDirectory(),
                    ["appsettingsJson"] = File.Exists(Path.Combine(projectDir, "appsettings.json")),
                    ["aspnetcoreEnv"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                }
            });
            File.AppendAllText(logPath, line + Environment.NewLine);
        }
        catch { /* agent log */ }
        // #endregion

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(projectDir)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        // #region agent log
        try
        {
            var logPath = Path.GetFullPath(Path.Combine(projectDir, "..", "debug-dcd861.log"));
            var ds = ExtractDataSource(connectionString);
            var line = JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["sessionId"] = "dcd861",
                ["hypothesisId"] = "H4",
                ["location"] = "AppDbContextDesignTimeFactory:after_config",
                ["message"] = "design_time_connection_meta",
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ["data"] = new Dictionary<string, object?>
                {
                    ["dataSource"] = ds,
                    ["configEnv"] = env
                }
            });
            File.AppendAllText(logPath, line + Environment.NewLine);
        }
        catch { /* agent log */ }
        // #endregion

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }

    private static string? ExtractDataSource(string connectionString)
    {
        foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var eq = part.IndexOf('=');
            if (eq <= 0) continue;
            var key = part[..eq].Trim();
            if (key.Equals("Server", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                return part[(eq + 1)..].Trim();
        }
        return null;
    }
}
