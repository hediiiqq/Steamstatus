using Steamstatus.Infrastructure.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Steamstatus.db.Interface;

namespace Steamstatus.db;

public class DbBaseCD : ITelegramDb<TelegramModel>
{
    private readonly AppDbContext db;

    private bool disposed = false;

    public DbBaseCD(AppDbContext db)
    {
        this.db = db;
    }
    public IEnumerable<TelegramModel> GetAllList()
    {
        return db.SubNotifies;
    }

    public TelegramModel? GetById(long id)
    {
        return db.SubNotifies.Find(id);
    }

    public bool Create(TelegramModel item)
    {

        if (db.SubNotifies.Find(item.Id) != null)
        {
            return false;
        }
        else
        {
            db.SubNotifies.Add(item);
            return true;
        }
    }

    public bool Delete(TelegramModel item)
    {
        TelegramModel? baseModel = db.SubNotifies.Find(item.Id);
        if (baseModel != null)
        {
            db.SubNotifies.Remove(baseModel);
            return true;
        }
        else return false;
    }

    public void SaveChanges()
    {
        db.SaveChanges();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                db.Dispose();
            }
        }

        this.disposed = true;
    }
}