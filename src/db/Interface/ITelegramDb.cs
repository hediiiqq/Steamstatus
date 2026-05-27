namespace Steamstatus.db.Interface;

public interface ITelegramDb<T> : IDisposable where T : class
{
    IEnumerable<T> GetAllList();
    T? GetById(long id);
    bool Create(T item);
    bool Delete(T item);
    void SaveChanges();
}