using MvvmDialogs;
using System;
using System.ComponentModel;
using System.Windows.Input;
using TaskManager.Core.Models;
using TaskManager.UI.Utils;

namespace TaskManager.UI.ViewModels
{
    public class AddEditTaskViewModel : INotifyPropertyChanged, IModalDialogViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // MvvmDialogs reads this to return from ShowDialog(...)
        public bool? DialogResult { get; private set; }

        public Action<bool> CloseAction { get; set; }  // keep this to actually close the window
        public bool IsEditMode { get; private set; }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                    (OkCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }

        public TaskItem Task { get; private set; }
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public AddEditTaskViewModel()
        {
            OkCommand = new RelayCommand(OnOk, CanOk);
            CancelCommand = new RelayCommand(OnCancel);
            IsEditMode = false;
            Title = "";
            Description = "";
            IsCompleted = false;
        }

        public AddEditTaskViewModel(TaskItem taskToEdit) : this()
        {
            if (taskToEdit == null) throw new ArgumentNullException(nameof(taskToEdit));
            IsEditMode = true;

            Title = taskToEdit.Title;
            Description = taskToEdit.Description;
            IsCompleted = taskToEdit.IsCompleted;

            Task = new TaskItem
            {
                Id = taskToEdit.Id,
                Title = taskToEdit.Title,
                Description = taskToEdit.Description,
                IsCompleted = taskToEdit.IsCompleted,
                CreatedAt = taskToEdit.CreatedAt
            };
        }

        private void OnOk()
        {
            if (Task == null)
            {
                Task = new TaskItem
                {
                    Title = Title,
                    Description = Description,
                    IsCompleted = IsCompleted,
                    CreatedAt = DateTime.Now
                };
            }
            else
            {
                Task.Title = Title;
                Task.Description = Description;
                Task.IsCompleted = IsCompleted;
            }

            DialogResult = true;      // <-- tell MvvmDialogs
            CloseAction?.Invoke(true); // <-- actually close the dialog
        }

        private bool CanOk() => !string.IsNullOrWhiteSpace(Title);

        private void OnCancel()
        {
            DialogResult = false;       // <-- tell MvvmDialogs
            CloseAction?.Invoke(false); // <-- close the dialog
        }

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}