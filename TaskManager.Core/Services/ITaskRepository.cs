using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.Core.Models;

namespace TaskManager.Core.Services
{
    public interface ITaskRepository
    {
        Task<IReadOnlyList<TaskItem>> GetAllAsync();
        Task AddAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(Guid taskId);
    }
}
