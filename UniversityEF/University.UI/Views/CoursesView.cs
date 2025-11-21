using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using University.UI.Dialogs;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Views;

public class CoursesView : BaseView
{
    private ListView _listView = null!;
    private List<Course> _courses = new();
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
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
            Height = Dim.Fill(3),
        };

        _listView.SelectedItemChanged += OnSelectionChanged;

        // First row of buttons
        var buttonY1 = Pos.AnchorEnd(2);

        _addButton = new Button("Add") { X = 1, Y = buttonY1 };
        _addButton.Clicked += OnAddClicked;

        _editButton = new Button("Edit")
        {
            X = Pos.Right(_addButton) + 1,
            Y = buttonY1,
            Enabled = false,
        };
        _editButton.Clicked += OnEditClicked;

        _deleteButton = new Button("Delete")
        {
            X = Pos.Right(_editButton) + 1,
            Y = buttonY1,
            Enabled = false,
        };
        _deleteButton.Clicked += OnDeleteClicked;

        // Second row
        var buttonY2 = Pos.AnchorEnd(1);

        _refreshButton = new Button("Refresh") { X = 1, Y = buttonY2 };
        _refreshButton.Clicked += async () => await LoadDataAsync();

        Add(_statusLabel, _listView, _addButton, _editButton, _deleteButton, _refreshButton);
    }

    private void OnSelectionChanged(ListViewItemEventArgs args)
    {
        var hasSelection = args.Item >= 0 && args.Item < _courses.Count;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
    }

    private async void OnAddClicked()
    {
        var dialog = new AddCourseDialog(ServiceProvider);
        TGuiApp.Run(dialog);

        if (dialog.Success)
        {
            await LoadDataAsync();
        }
    }

    private async void OnEditClicked()
    {
        if (_listView.SelectedItem < 0 || _listView.SelectedItem >= _courses.Count)
            return;

        var course = _courses[_listView.SelectedItem];
        var dialog = new UpdateCourseDialog(ServiceProvider, course);
        TGuiApp.Run(dialog);

        if (dialog.Success)
        {
            await LoadDataAsync();
        }
    }

    private async void OnDeleteClicked()
    {
        if (_listView.SelectedItem < 0 || _listView.SelectedItem >= _courses.Count)
            return;

        var course = _courses[_listView.SelectedItem];

        var confirm = MessageBox.Query(
            "Confirm Delete",
            $"Delete course:\n{course.Name} ({course.CourseCode})?",
            "Yes",
            "No"
        );

        if (confirm == 0)
        {
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();
                await courseService.DeleteCourseAsync(course.Id);
                MessageBox.Query("Success", "Course deleted successfully!", "OK");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"Failed to delete course:\n{ex.Message}", "OK");
            }
        }
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
