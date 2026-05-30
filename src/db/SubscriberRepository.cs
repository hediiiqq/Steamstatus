using Steamstatus.Infrastructure.Telegram;
using Steamstatus.db.Interface;

namespace Steamstatus.db;

public class SubscriberRepository : ISubscriberRepository<TelegramModel>
{
    private readonly AppDbContext db;


    public SubscriberRepository(AppDbContext db)
    {
        this.db = db;
    }

    public IEnumerable<TelegramModel> GetAll()
    {
        return db.SubNotifies;
    }

    public IEnumerable<TelegramModel> GetByChatId(long chatId)
    {
        return db.SubNotifies.Where(x => x.ChatId == chatId);
    }

    public IEnumerable<TelegramModel> GetByServiceName(string serviceName)
    {
        return db.SubNotifies.Where(x => x.ServiceName == serviceName);
    }

    public bool Exists(long chatId, string serviceName)
    {
        if (db.SubNotifies.Any(x =>
                x.ChatId == chatId && x.ServiceName == serviceName))
        {
            return true;
        }

        return false;
    }

    public bool Create(long chatId, string serviceName)
    {
        if (Exists(chatId, serviceName))
        {
            return false;
        }

        var subscriber = new TelegramModel()
        {
            ChatId = chatId,
            ServiceName = serviceName
        };

        db.SubNotifies.Add(subscriber);
        return true;
    }

    public bool Delete(long chatId, string serviceName)
    {
        var subscriber = db.SubNotifies.FirstOrDefault(x =>
            x.ChatId == chatId &&
            x.ServiceName == serviceName);

        if (subscriber == null)
        {
            return false;
        }

        db.SubNotifies.Remove(subscriber);
        return true;
    }

    public void SaveChanges()
    {
        db.SaveChanges();
    }
}