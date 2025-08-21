# Task Manager (WPF, MVVM)

A simple WPF desktop app to manage tasks with add/edit/delete, filtering, and PDF export.

## Features
- View tasks with filter: All / Completed / Not Completed
- Add, edit, and delete tasks in a modal dialog
- Export the current (filtered) list to PDF
- File-backed storage (`tasks.json`)

## Architecture (Why & How)
- **MVVM**: Views are XAML-only; ViewModels expose properties and `ICommand`s; Models live in Core.
- **Navigation**: Dialogs are opened from the ViewModel via **MvvmDialogs** (`IDialogService.ShowDialog(ownerVm, dialogVm)`). The dialog VM implements `IModalDialogViewModel` to return a result.
- **Dependency Injection**: `Microsoft.Extensions.DependencyInjection` configures `ITaskRepository`, `IDialogService` (MvvmDialogs), and ViewModels at app startup.
- **Persistence**: `ITaskRepository` implemented by `FileTaskRepository` (JSON file).
- **Export**: **QuestPDF** renders a PDF; license set once in a static constructor.

## Solution Structure
~~~
TaskManager.sln
├─ TaskManager.Core
│ ├─ Models/TaskItem.cs
│ └─ Services/ITaskRepository.cs
├─ TaskManager.Infrastructure
│ ├─ Repositories/FileTaskRepository.cs
│ └─ Export/PdfExporter.cs
├─ TaskManager.UI
│ ├─ Views/MainWindow.xaml, AddEditTask.xaml
│ ├─ ViewModels/MainViewModel.cs, AddEditTaskViewModel.cs
│ ├─ App.xaml, App.xaml.cs
└─└─ Utils/RelayCommand.cs
~~~

## Getting Started

### Prerequisites
- .NET SDK 8 (or 6/7 if your `.csproj` targets that)
- Windows (WPF)

### Build & Run
```bash
git clone https://github.com/<you>/<repo>.git
cd <repo>
dotnet restore
dotnet build
# From Visual Studio: press F5
# Or run:
# dotnet run --project TaskManager.UI
