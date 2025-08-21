using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TaskManager.Core.Models;
using TaskManager.Core.Services;

namespace TaskManager.Infrastructure.Export
{
    public class PdfExporter : IPdfExporter
    {
        static PdfExporter()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public void ExportTasks(IEnumerable<TaskItem> tasks, string filePath)
        {
            Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Task List").FontSize(20).SemiBold().AlignCenter();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.ConstantColumn(80);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Element(Cell).Text("Title").SemiBold();
                            h.Cell().Element(Cell).Text("Description").SemiBold();
                            h.Cell().Element(Cell).Text("Completed").SemiBold();
                        });
                        foreach (var t in tasks)
                        {
                            table.Cell().Element(Cell).Text(t.Title);
                            table.Cell().Element(Cell).Text(t.Description);
                            table.Cell().Element(Cell).Text(t.IsCompleted ? "Yes" : "No");
                        }
                    });
                });
            }).GeneratePdf(filePath);
        }

        private IContainer Cell(IContainer container) => container.Padding(5);
    }
}