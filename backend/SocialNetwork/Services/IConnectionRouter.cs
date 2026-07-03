namespace SocialNetwork.Services;

/// <summary>
/// Выбирает строку подключения: запись — на мастер, чтение — на слейвы (round-robin).
/// Аналог ReplicationRoutingDataSource из Java/Spring, но для ADO.NET.
/// </summary>
public interface IConnectionRouter
{
    /// <summary>Строка подключения для записи (всегда мастер)</summary>
    string GetWriteConnectionString();

    /// <summary>Строка подключения для чтения (слейв round-robin, либо мастер если реплики выключены)</summary>
    string GetReadConnectionString();
}
