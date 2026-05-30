namespace Steamstatus.Infrastructure.Telegram;

public class TelegramModel
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
}