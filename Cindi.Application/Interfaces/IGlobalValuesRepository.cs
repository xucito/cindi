using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cindi.Domain.Entities.GlobalValues;

namespace Cindi.Application.Interfaces
{
    public interface IGlobalValuesRepository
    {
        Task<List<GlobalValue>> GetGlobalValuesAsync(int size = 10, int page = 0, string sortOn = "name", string sortDirection = "asc");
        Task<GlobalValue> GetGlobalValueAsync(string globalValueName);
        Task<GlobalValue> GetGlobalValueAsync(Guid id);
        Task<GlobalValue> InsertGlobalValue(GlobalValue globalValue);
    }
}