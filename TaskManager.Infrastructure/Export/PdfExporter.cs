using System.Collections.Generic;
using QuestPDF.Fluent;
using TaskManager.Core.Models;
using QuestPDF.Infrastructure;

namespace TaskManager.Infrastructure.Export
{
    public class PdfExporter
    {
        static PdfExporter()
        {
            // Set the QuestPDF license once, before the first export.
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            // If you have a commercial license, use: LicenseType.Professional
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
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.ConstantColumn(80);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Title").SemiBold();
                            header.Cell().Element(CellStyle).Text("Description").SemiBold();
                            header.Cell().Element(CellStyle).Text("Completed").SemiBold();
                        });

                        foreach (var task in tasks)
                        {
                            table.Cell().Element(CellStyle).Text(task.Title);
                            table.Cell().Element(CellStyle).Text(task.Description);
                            table.Cell().Element(CellStyle).Text(task.IsCompleted ? "Yes" : "No");
                        }
                    });
                });
            }).GeneratePdf(filePath);
        }

        private IContainer CellStyle(IContainer container) => container.Padding(5);
    }
}