using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class UpdateCourseDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private readonly int _courseId;
    private readonly TextField _nameField;
    private readonly TextField _codeField;
    private readonly TextField _ectsField;
    public bool Success { get; private set; }

    public UpdateCourseDialog(IServiceProvider serviceProvider, Course course)
    {
        _serviceProvider = serviceProvider;
        _courseId = course.Id;
        Title = $"Update Course (ID: {course.Id})";
        Width = 70;
        Height = 14;

        var deptLabel = new Label($"Department: {course.Department.Name}") { X = 1, Y = 1 };

        var nameLabel = new Label("Course Name:") { X = 1, Y = 2 };
        _nameField = new TextField(course.Name)
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill(1),
        };

        var codeLabel = new Label("Course Code:") { X = 1, Y = 4 };
        _codeField = new TextField(course.CourseCode)
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill(1),
        };

        var ectsLabel = new Label("ECTS Points:") { X = 1, Y = 6 };
        _ectsField = new TextField(course.ECTSPoints.ToString())
        {
            X = 1,
            Y = 7,
            Width = Dim.Fill(1),
        };

        var saveButton = new Button("Update")
        {
            X = 1,
            Y = 9,
            IsDefault = true,
        };
        saveButton.Clicked += OnUpdate;

        var cancelButton = new Button("Cancel") { X = Pos.Right(saveButton) + 2, Y = 9 };
        cancelButton.Clicked += () => TGuiApp.RequestStop();

        Add(
            deptLabel,
            nameLabel,
            _nameField,
            codeLabel,
            _codeField,
            ectsLabel,
            _ectsField,
            saveButton,
            cancelButton
        );
    }

    private async void OnUpdate()
    {
        var name = _nameField.Text.ToString()?.Trim();
        var code = _codeField.Text.ToString()?.Trim();
        var ectsText = _ectsField.Text.ToString()?.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(code))
        {
            MessageBox.ErrorQuery("Validation Error", "Name and Code are required!", "OK");
            return;
        }

        if (!int.TryParse(ectsText, out int ects) || ects < 1)
        {
            MessageBox.ErrorQuery("Validation Error", "ECTS must be a positive number!", "OK");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();

            // Get fresh instance from database to avoid tracking conflicts
            var course = await courseService.GetCourseByIdAsync(_courseId);
            if (course == null)
            {
                MessageBox.ErrorQuery("Error", "Course not found!", "OK");
                return;
            }

            course.Name = name;
            course.CourseCode = code;
            course.ECTSPoints = ects;

            await courseService.UpdateCourseAsync(course);

            Success = true;
            MessageBox.Query("Success", "Course updated successfully!", "OK");
            TGuiApp.RequestStop();
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to update course:\n{ex.Message}", "OK");
        }
    }
}
