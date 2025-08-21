using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using TaskManager.Core.Models;
using TaskManager.Core.Services;
using TaskManager.UI.Utils;
using TaskManager.UI.Views;

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
                TasksView.Refresh();
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
                (EditCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
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

            Tasks = new ObservableCollection<TaskItem>(_repository.GetAll());
            TasksView = CollectionViewSource.GetDefaultView(Tasks);
            TasksView.Filter = FilterTask;

            AddCommand = new RelayCommand(OnAdd);
            EditCommand = new RelayCommand(OnEdit, () => SelectedTask != null);
            DeleteCommand = new RelayCommand(OnDelete, () => SelectedTask != null);
            ExportCommand = new RelayCommand(OnExport);
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

        private void OnAdd()
        {
            var vm = new AddEditTaskViewModel();
            bool? ok = _dialogService.ShowDialog(this, vm);
            if (ok == true)
            {
                var newTask = vm.Task;
                _repository.Add(newTask);
                Tasks.Add(newTask);
            }
        }

        private void OnEdit()
        {
            if (SelectedTask == null) return;
            var vm = new AddEditTaskViewModel(SelectedTask);
            bool? ok = _dialogService.ShowDialog(this, vm);
            if (ok == true)
            {
                _repository.Update(vm.Task);
                var idx = Tasks.IndexOf(SelectedTask);
                if (idx >= 0) Tasks[idx] = vm.Task;
                TasksView.Refresh();
            }
        }

        private void OnDelete()
        {
            var answer = _dialogService.ShowMessageBox(this, new MessageBoxSettings
            {
                MessageBoxText = "Are you sure you want to delete this task?",
                Caption = "Confirm Delete",
                Button = System.Windows.MessageBoxButton.YesNo,
                Icon = System.Windows.MessageBoxImage.Question
            });
            if (answer == System.Windows.MessageBoxResult.Yes && SelectedTask != null)
            {
                _repository.Delete(SelectedTask.Id);
                Tasks.Remove(SelectedTask);
            }
        }

        private void OnExport()
        {
            var settings = new SaveFileDialogSettings
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = "Tasks.pdf",
                AddExtension = true,
                DefaultExt = ".pdf",
                OverwritePrompt = true
            };

            bool? save = _dialogService.ShowSaveFileDialog(this, settings);
            if (save == true)
            {
                var visible = TasksView.Cast<TaskItem>().ToList();

                try
                {
                    _pdfExporter.ExportTasks(visible, settings.FileName); // <-- read path from settings
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
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}