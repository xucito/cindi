using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cindi.Domain.Entities.BotKeys;

namespace Cindi.Application.Interfaces
{
    public interface IBotKeysRepository
    {
        Task<bool> DeleteBotkey(Guid id);
        Task<BotKey> GetBotKeyAsync(Guid id);
        Task<List<BotKey>> GetBotKeysAsync(int size = 10, int page = 0);
        Task<BotKey> InsertBotKeyAsync(BotKey key);
    }
}