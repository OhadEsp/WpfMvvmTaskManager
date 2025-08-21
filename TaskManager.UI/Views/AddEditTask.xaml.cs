using System.Windows;
using TaskManager.UI.ViewModels;

namespace TaskManager.UI.Views
{
    public partial class AddEditTask : Window
    {
        public AddEditTask()
        {
            InitializeComponent();

            // Attach close action when DataContext is set
            this.DataContextChanged += (s, e) =>
            {
                if (this.DataContext is AddEditTaskViewModel vm)
                    vm.CloseAction = result =>
                    {
                        this.DialogResult = result;
                        this.Close();
                    };
            };
        }

        public AddEditTaskViewModel ViewModel
        {
            get => (AddEditTaskViewModel)DataContext;
            set => DataContext = value;
        }
    }
}