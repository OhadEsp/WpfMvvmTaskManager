using MvvmDialogs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TaskManager.Core.Models;
using TaskManager.Core.Services;
using TaskManager.Infrastructure.Export;
using TaskManager.UI.Utils;
using TaskManager.UI.Views;

namespace TaskManager.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IModalDialogViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly ITaskRepository _repository;
        private readonly PdfExporter _pdfExporter;

        public ObservableCollection<TaskItem> Tasks { get; set; }
        public ObservableCollection<TaskItem> FilteredTasks { get; set; }
        public ObservableCollection<string> StatusFilters { get; }
        private string _selectedFilter;
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (_selectedFilter != value)
                {
                    _selectedFilter = value;
                    OnPropertyChanged(nameof(SelectedFilter));
                    ApplyFilter();
                }
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

        public bool? DialogResult { get; private set; }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExportCommand { get; }

        public MainViewModel(ITaskRepository repository, IDialogService dialogService)
        {
            _repository = repository;
            _dialogService = dialogService;
            _pdfExporter = new PdfExporter();

            Tasks = new ObservableCollection<TaskItem>(_repository.GetAll());
            FilteredTasks = new ObservableCollection<TaskItem>(Tasks);

            StatusFilters = new ObservableCollection<string> { "All", "Completed", "Not Completed" };
            SelectedFilter = "All";

            AddCommand = new RelayCommand(OnAdd);
            EditCommand = new RelayCommand(OnEdit, () => SelectedTask != null);
            DeleteCommand = new RelayCommand(OnDelete, () => SelectedTask != null);
            ExportCommand = new RelayCommand(OnExport);

            Tasks.CollectionChanged += (s, e) => ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredTasks.Clear();
            var filtered = SelectedFilter switch
            {
                "Completed" => Tasks.Where(t => t.IsCompleted),
                "Not Completed" => Tasks.Where(t => !t.IsCompleted),
                _ => Tasks
            };

            foreach (var task in filtered)
                FilteredTasks.Add(task);
        }

        private void OnAdd()
        {
            var vm = new AddEditTaskViewModel();
            bool? success = _dialogService.ShowDialog(this, vm);
            if (success == true)
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
            bool? success = _dialogService.ShowDialog(this, vm);
            if (success == true)
            {
                _repository.Update(vm.Task);
                var idx = Tasks.IndexOf(SelectedTask);
                if (idx >= 0)
                    Tasks[idx] = vm.Task;
                ApplyFilter();
            }
        }

        private void OnDelete()
        {
            if (SelectedTask == null) return;
            if (MessageBox.Show("Are you sure you want to delete this task?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _repository.Delete(SelectedTask.Id);
                Tasks.Remove(SelectedTask);
            }
        }

        private void OnExport()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = "Tasks.pdf"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _pdfExporter.ExportTasks(FilteredTasks, dialog.FileName);
                    MessageBox.Show("Export successful!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion
    }
}