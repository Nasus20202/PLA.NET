using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Views;

public class CoursesView : BaseView
{
    private ListView _listView = null!;
    private List<Course> _courses = new();
    private Button _refreshButton = null!;
    private Label _statusLabel = null!;

    public CoursesView(IServiceProvider serviceProvider)
        : base(serviceProvider, "Courses")
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        _statusLabel = new Label("Loading...")
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill(1),
        };

        _listView = new ListView()
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(2),
        };

        var buttonY = Pos.AnchorEnd(1);

        _refreshButton = new Button("Refresh") { X = 1, Y = buttonY };
        _refreshButton.Clicked += async () => await LoadDataAsync();

        Add(_statusLabel, _listView, _refreshButton);
    }

    public override async Task LoadDataAsync()
    {
        try
        {
            _statusLabel.Text = "Loading courses...";
            TGuiApp.MainLoop.Invoke(() => SetNeedsDisplay());

            using var scope = ServiceProvider.CreateScope();
            var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();
            _courses = (await courseService.GetAllCoursesAsync()).ToList();

            TGuiApp.MainLoop.Invoke(() =>
            {
                var items = _courses
                    .Select(c =>
                        $"ID:{c.Id, 4} | {c.CourseCode, -8} | {c.Name, -40} | {c.ECTSPoints, 2} ECTS | Dept: {c.Department.Name}"
                    )
                    .ToList();

                _listView.SetSource(items);
                _statusLabel.Text = $"Total courses: {_courses.Count}";
                SetNeedsDisplay();
            });
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                _statusLabel.Text = "Error loading courses";
                MessageBox.ErrorQuery("Error", $"Failed to load courses:\n{ex.Message}", "OK");
            });
        }
    }
}
