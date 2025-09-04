using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
            // Configure QuestPDF license once
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public Task ExportTasksAsync(IEnumerable<TaskItem> tasks, string filePath, CancellationToken ct = default)
        {
            // QuestPDF is synchronous; run on a background thread to keep UI free
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

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
                                h.Cell().Padding(5).Text("Title").SemiBold();
                                h.Cell().Padding(5).Text("Description").SemiBold();
                                h.Cell().Padding(5).Text("Completed").SemiBold();
                            });

                            foreach (var t in tasks)
                            {
                                table.Cell().Padding(5).Text(t.Title);
                                table.Cell().Padding(5).Text(t.Description);
                                table.Cell().Padding(5).Text(t.IsCompleted ? "Yes" : "No");
                            }
                        });
                    });
                }).GeneratePdf(filePath);
            }, ct);
        }
    }
}
