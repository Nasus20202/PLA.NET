using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class CourseAveragesDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private ListView _listView = null!;
    private List<Department> _departments = new();
    private Button _btnRun = null!;
    private Button _btnClose = null!;

    public CourseAveragesDialog(IServiceProvider serviceProvider)
        : base("Course Averages by Department", 70, 20)
    {
        _serviceProvider = serviceProvider;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        var label = new Label("Select Department:") { X = 2, Y = 1 };

        _listView = new ListView()
        {
            X = 2,
            Y = 2,
            Width = Dim.Fill(2),
            Height = 8,
        };

        _btnRun = new Button("Run Query") { X = Pos.Center() - 8, Y = 11 };
        _btnRun.Clicked += OnRunClicked;

        _btnClose = new Button("Close") { X = Pos.Center() + 8, Y = 11 };
        _btnClose.Clicked += () => TGuiApp.RequestStop();

        Add(label, _listView, _btnRun, _btnClose);
    }

    public async Task LoadDepartmentsAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var departmentService = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
            _departments = (await departmentService.GetAllDepartmentsAsync()).ToList();

            var deptList = _departments.Select(d => $"{d.Id}: {d.Name}").ToList();
            _listView.SetSource(deptList);
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to load departments:\n{ex.Message}", "OK");
            TGuiApp.RequestStop();
        }
    }

    private async void OnRunClicked()
    {
        if (_listView.SelectedItem < 0 || _listView.SelectedItem >= _departments.Count)
        {
            MessageBox.ErrorQuery("Error", "Please select a department!", "OK");
            return;
        }

        var selectedDept = _departments[_listView.SelectedItem];

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var queryService = scope.ServiceProvider.GetRequiredService<IQueryService>();
            var results = await queryService.GetCourseAveragesForFacultyAsync(selectedDept.Id);

            var resultText = string.Join(
                "\n",
                results.Select(r =>
                    $"{r.CourseCode} - {r.CourseName}: Avg={r.AverageGrade:F2}, Students={r.StudentCount}"
                )
            );

            if (string.IsNullOrEmpty(resultText))
                resultText = "No courses with grades found for this department.";

            MessageBox.Query($"Course Averages - {selectedDept.Name}", resultText, "OK");
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Query failed:\n{ex.Message}", "OK");
        }
    }
}
