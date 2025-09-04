using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using TaskManager.Core.Models;
using TaskManager.Core.Services;
using TaskManager.UI.Utils;

namespace TaskManager.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ITaskRepository _repository;
        private readonly IDialogService _dialogService;
        private readonly IPdfExporter _pdfExporter;

        public ObservableCollection<TaskItem> Tasks { get; }
        public ICollectionView TasksView { get; }
        public ObservableCollection<string> StatusFilters { get; } = new() { "All", "Completed", "Not Completed" };

        private string _selectedFilter = "All";
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (_selectedFilter == value) return;
                _selectedFilter = value;
                OnPropertyChanged(nameof(SelectedFilter));
                TasksView?.Refresh();
            }
        }

        private TaskItem _selectedTask;
        public TaskItem SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                OnPropertyChanged(nameof(SelectedTask));
                RaiseCmds();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                RaiseCmds();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExportCommand { get; }

        public MainViewModel(ITaskRepository repository, IDialogService dialogService, IPdfExporter pdfExporter)
        {
            _repository = repository;
            _dialogService = dialogService;
            _pdfExporter = pdfExporter;

            Tasks = new ObservableCollection<TaskItem>();
            TasksView = CollectionViewSource.GetDefaultView(Tasks);
            TasksView.Filter = FilterTask;

            AddCommand = new AsyncRelayCommand(OnAddAsync, () => !IsBusy);
            EditCommand = new AsyncRelayCommand(OnEditAsync, () => SelectedTask != null && !IsBusy);
            DeleteCommand = new AsyncRelayCommand(OnDeleteAsync, () => SelectedTask != null && !IsBusy);
            ExportCommand = new AsyncRelayCommand(OnExportAsync, () => !IsBusy);

            // Fire-and-forget initial load (remains on UI ctx after awaits)
            _ = LoadAsync();
        }

        private bool FilterTask(object obj)
        {
            if (obj is not TaskItem t) return false;
            return SelectedFilter switch
            {
                "Completed" => t.IsCompleted,
                "Not Completed" => !t.IsCompleted,
                _ => true
            };
        }

        private async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                var items = await _repository.GetAllAsync();
                Tasks.Clear();
                foreach (var t in items) Tasks.Add(t);
                TasksView.Refresh();
            }
            finally { IsBusy = false; }
        }

        private async Task OnAddAsync()
        {
            var vm = new AddEditTaskViewModel();
            bool? ok = _dialogService.ShowDialog(this, vm);
            if (ok == true)
            {
                await _repository.AddAsync(vm.Task);
                Tasks.Add(vm.Task);
                TasksView.Refresh();
            }
        }

        private async Task OnEditAsync()
        {
            if (SelectedTask == null) return;
            var vm = new AddEditTaskViewModel(SelectedTask);
            bool? ok = _dialogService.ShowDialog(this, vm);
            if (ok == true)
            {
                await _repository.UpdateAsync(vm.Task);
                var idx = Tasks.IndexOf(SelectedTask);
                if (idx >= 0) Tasks[idx] = vm.Task;
                TasksView.Refresh();
            }
        }

        private async Task OnDeleteAsync()
        {
            var result = _dialogService.ShowMessageBox(this, new MessageBoxSettings
            {
                MessageBoxText = "Are you sure you want to delete this task?",
                Caption = "Confirm Delete",
                Button = System.Windows.MessageBoxButton.YesNo,
                Icon = System.Windows.MessageBoxImage.Question
            });

            if (result == System.Windows.MessageBoxResult.Yes && SelectedTask != null)
            {
                await _repository.DeleteAsync(SelectedTask.Id);
                Tasks.Remove(SelectedTask);
            }
        }

        private async Task OnExportAsync()
        {
            var settings = new SaveFileDialogSettings
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = "Tasks.pdf",
                AddExtension = true,
                DefaultExt = ".pdf",
                OverwritePrompt = true
            };

            bool? ok = _dialogService.ShowSaveFileDialog(this, settings);
            if (ok == true)
            {
                try
                {
                    IsBusy = true;
                    var visible = TasksView.Cast<TaskItem>().ToList();
                    await _pdfExporter.ExportTasksAsync(visible, settings.FileName);
                    _dialogService.ShowMessageBox(this, new MessageBoxSettings
                    {
                        MessageBoxText = "Export successful!",
                        Caption = "Export",
                        Button = System.Windows.MessageBoxButton.OK,
                        Icon = System.Windows.MessageBoxImage.Information
                    });
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessageBox(this, new MessageBoxSettings
                    {
                        MessageBoxText = $"Export failed: {ex.Message}",
                        Caption = "Error",
                        Button = System.Windows.MessageBoxButton.OK,
                        Icon = System.Windows.MessageBoxImage.Error
                    });
                }
                finally { IsBusy = false; }
            }
        }

        private void RaiseCmds()
        {
            (AddCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (EditCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (DeleteCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (ExportCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}