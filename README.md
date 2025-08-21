# Task Manager (WPF, MVVM)

A simple WPF desktop app to manage tasks with add/edit/delete, filtering, and PDF export.

## Features
- View tasks with filter: All / Completed / Not Completed
- Add, edit, and delete tasks in a modal dialog
- Export the current (filtered) list to PDF
- File-backed storage (`tasks.json`)

## Architecture & Technical Decisions

### MVVM
- **Views**: XAML-only (no business logic in code-behind).  
- **ViewModels**: expose properties and `ICommand`s, implement `INotifyPropertyChanged`.  
- **Models**: simple POCOs in the **Core** project.

### Navigation / Dialogs
- Uses **[MvvmDialogs](https://github.com/FantasticFiasco/mvvm-dialogs)**.  
  - `MainViewModel` calls `IDialogService.ShowDialog(ownerVm, dialogVm)` to open the **Add/Edit** dialog.  
  - The dialog ViewModel (`AddEditTaskViewModel`) implements `IModalDialogViewModel` and sets `DialogResult` in **OK/Cancel**.
  - The dialog view wires a `CloseAction` to set `Window.DialogResult` and close itself.  
- Framework dialogs (Confirm Delete, Save File) use **MvvmDialogs** wrappers (`ShowMessageBox`, `ShowSaveFileDialog`) so ViewModels remain UI-agnostic.

### Dependency Injection
- **Microsoft.Extensions.DependencyInjection** composes the app at startup.  
- Registered services include:
  - `ITaskRepository` → `FileTaskRepository` (JSON storage)
  - `IDialogService` (MvvmDialogs)
  - `IPdfExporter` → `PdfExporter` (QuestPDF)
  - ViewModels and `MainWindow`
- `App.xaml.cs` builds the container, resolves `MainWindow`, and sets `DataContext` to `MainViewModel`.

### Persistence
- `ITaskRepository` abstracts storage; `FileTaskRepository` reads/writes `tasks.json` using `System.Text.Json`.
- (Optionally) store under `%LocalAppData%\TaskManager\tasks.json` for a stable path.

### Filtering
- Uses **`ICollectionView`** (`CollectionViewSource.GetDefaultView(Tasks)`) with a `Filter` predicate.  
  This avoids maintaining a separate list and enables future sort/group.

### Export
- `IPdfExporter` abstracts export; `PdfExporter` uses **QuestPDF** to generate a simple table PDF.
- License set once via static ctor (`LicenseType.Community`), no extra setup required.

---

## Solution Structure

```
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
```

---

## Getting Started

### Prerequisites
- Windows
- .NET SDK (6/7/8; match the project’s `TargetFramework`)
- Visual Studio 2022+ (recommended) or `dotnet` CLI

### Build & Run (Visual Studio)
1. Open \`TaskManager.sln\`
2. Set **TaskManager.UI** as the Startup Project
3. Press **F5**

### Build & Run (CLI)
```bash
dotnet restore
dotnet build
dotnet run --project TaskManager.UI
```

---

## How to Use

1. **Add** a task → click **Add**, enter Title/Description, **OK**.  
2. **Edit** a task → select a row, click **Edit**, **OK**.  
3. **Delete** a task → select a row, click **Delete** (confirm in dialog).  
4. **Filter** → choose **All / Completed / Not Completed** above the list.  
5. **Export** → click **Export** and choose a \`.pdf\` destination.

> Export saves the *currently visible (filtered)* tasks.

---

## Notes & Decisions (Short Form)

- **Dialogs**: MvvmDialogs keeps VMs decoupled from WPF types.  
- **Filtering**: \`ICollectionView\` instead of duplicating collections.  
- **DI**: all services (repo, dialog service, exporter) are constructor-injected.  
- **Storage**: JSON file; simple and testable.  
- **PDF**: QuestPDF with a basic table layout; community license configured once.

---

## Known Limitations / Next Steps

- Persist to \`%LocalAppData%/TaskManager\` by default (if you aren’t already).  
- Add input validation UI (\`IDataErrorInfo\` / \`INotifyDataErrorInfo\`) for Title.  
- Enhance PDF styling (headers, line wrapping, pagination).  
- Add unit tests to demonstrate testability.

---

## Repository

GitHub: **https://github.com/OhadEsp/WpfMvvmTaskManager.git**

---

## License

This project is provided for demonstration/interview purposes.  
QuestPDF is used under its Community license.