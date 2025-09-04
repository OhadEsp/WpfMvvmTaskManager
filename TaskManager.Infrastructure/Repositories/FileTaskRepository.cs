using System;
using System.Collections.Generic;
using System.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaskManager.Core.Models;
using TaskManager.Core.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskManager.Infrastructure.Repositories
{
    public class FileTaskRepository : ITaskRepository
    {
        private static readonly string RootDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TaskManager");

        private readonly string _filePath = Path.Combine(RootDir, "tasks.json");
        
        // Used for backward compatibility.
        private readonly string _legacyRelativePath = "tasks.json";

        private readonly SemaphoreSlim _gate = new(1, 1);
        private List<TaskItem> _tasks = new();
        private bool _initialized;

        public async Task<IReadOnlyList<TaskItem>> GetAllAsync()
        {
            await EnsureLoadedAsync().ConfigureAwait(false);
            // return a copy to keep internal list safe
            return _tasks.ToArray();
        }

        public async Task AddAsync(TaskItem task)
        {
            await EnsureLoadedAsync().ConfigureAwait(false);
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                _tasks.Add(task);
                await SaveAsync().ConfigureAwait(false);
            }
            finally { _gate.Release(); }
        }

        public async Task UpdateAsync(TaskItem task)
        {
            await EnsureLoadedAsync().ConfigureAwait(false);
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var idx = _tasks.FindIndex(t => t.Id == task.Id);
                if (idx >= 0)
                {
                    _tasks[idx] = task;
                    await SaveAsync().ConfigureAwait(false);
                }
            }
            finally { _gate.Release(); }
        }

        public async Task DeleteAsync(Guid taskId)
        {
            await EnsureLoadedAsync().ConfigureAwait(false);
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                _tasks.RemoveAll(t => t.Id == taskId);
                await SaveAsync().ConfigureAwait(false);
            }
            finally { _gate.Release(); }
        }

        private async Task EnsureLoadedAsync()
        {
            if (_initialized) return;
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_initialized) return;
                Directory.CreateDirectory(RootDir);

                // one-time migration from legacy working-dir file
                if (!File.Exists(_filePath) && File.Exists(_legacyRelativePath))
                {
                    try { File.Copy(_legacyRelativePath, _filePath, overwrite: false); }
                    catch { /* ignore */ }
                }

                if (File.Exists(_filePath))
                {
                    await using var fs = File.OpenRead(_filePath);
                    _tasks = (await JsonSerializer.DeserializeAsync<List<TaskItem>>(fs).ConfigureAwait(false)) ?? new();
                }
                else
                {
                    _tasks = new List<TaskItem>();
                    await SaveAsync().ConfigureAwait(false);
                }

                _initialized = true;
            }
            finally { _gate.Release(); }
        }

        private async Task SaveAsync()
        {
            Directory.CreateDirectory(RootDir);
            await using var fs = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(fs, _tasks, new JsonSerializerOptions { WriteIndented = true })
                                 .ConfigureAwait(false);
        }
    }
}