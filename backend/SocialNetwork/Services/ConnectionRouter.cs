namespace SocialNetwork.Services;

public class ConnectionRouter : IConnectionRouter
{
    private readonly string _master;
    private readonly string[] _slaves;
    private readonly bool _useReplicas;
    private int _counter = -1;

    public ConnectionRouter(IConfiguration configuration)
    {
        _master = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not configured");

        _useReplicas = configuration.GetValue<bool>("Replication:UseReplicas");
        _slaves = configuration.GetSection("Replication:Slaves").Get<string[]>() ?? [];
    }

    public string GetWriteConnectionString() => _master;

    public string GetReadConnectionString()
    {
        if (!_useReplicas || _slaves.Length == 0)
            return _master;

        // Round-robin по слейвам
        var idx = (uint)Interlocked.Increment(ref _counter) % (uint)_slaves.Length;
        return _slaves[idx];
    }
}
