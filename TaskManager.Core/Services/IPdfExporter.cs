using System.Collections.Generic;
using TaskManager.Core.Models;

namespace TaskManager.Core.Services
{
    public interface IPdfExporter
    {
        void ExportTasks(IEnumerable<TaskItem> tasks, string filePath);
    }
}