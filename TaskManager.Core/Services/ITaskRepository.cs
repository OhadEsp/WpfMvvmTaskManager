using TaskManager.Core.Models;

namespace TaskManager.Core.Services
{
    public interface ITaskRepository
    {
        IEnumerable<TaskItem> GetAll();
        void Add(TaskItem task);
        void Update(TaskItem task);
        void Delete(Guid taskId);
    }
}
