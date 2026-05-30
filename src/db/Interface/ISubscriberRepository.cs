namespace Steamstatus.db.Interface;

public interface ISubscriberRepository<T> where T : class
{
    IEnumerable<T> GetAll();
    IEnumerable<T> GetByChatId(long chatId);
    IEnumerable<T> GetByServiceName(string serviceName);
    bool Exists(long chatId, string serviceName);
    bool Create(long chatId, string serviceName);
    bool Delete(long chatId, string serviceName);
    void SaveChanges();
}