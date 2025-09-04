using System.Collections.Generic;
using TaskManager.Core.Models;

namespace TaskManager.Core.Services
{
    public interface IPdfExporter
    {
        Task ExportTasksAsync(IEnumerable<TaskItem> tasks, string filePath, CancellationToken ct = default);
    }
}