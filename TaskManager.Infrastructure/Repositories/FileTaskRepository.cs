using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TaskManager.Core.Models;
using TaskManager.Core.Services;

namespace TaskManager.Infrastructure.Repositories
{
    public class FileTaskRepository : ITaskRepository
    {
        private readonly string _filePath = "tasks.json";
        private List<TaskItem> _tasks = new();

        public FileTaskRepository()
        {
            Load();
        }

        public IEnumerable<TaskItem> GetAll() => _tasks;

        public void Add(TaskItem task)
        {
            _tasks.Add(task);
            Save();
        }

        public void Update(TaskItem task)
        {
            var idx = _tasks.FindIndex(t => t.Id == task.Id);
            if (idx >= 0)
            {
                _tasks[idx] = task;
                Save();
            }
        }

        public void Delete(Guid taskId)
        {
            _tasks.RemoveAll(t => t.Id == taskId);
            Save();
        }

        private void Load()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _tasks = JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new();
            }
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_tasks, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}