using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cindi.Domain.Entities.Users;

namespace Cindi.Application.Interfaces
{
    public interface IUsersRepository
    {
        Task<User> InsertUserAsync(User user);
        Task<User> GetUserAsync(string username);
        Task<User> GetUserAsync(Guid id);
        Task<List<User>> GetUsersAsync(int size = 10, int page = 0);
        Task<bool> DeleteUser(string username);
        long CountUsers();
    }
}