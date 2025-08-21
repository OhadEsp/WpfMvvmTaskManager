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
