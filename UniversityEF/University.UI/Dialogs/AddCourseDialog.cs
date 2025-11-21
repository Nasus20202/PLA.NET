using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class AddCourseDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TextField _nameField;
    private readonly TextField _codeField;
    private readonly TextField _ectsField;
    private readonly TextField _departmentIdField;
    private readonly TextField _professorIdField;
    public bool Success { get; private set; }

    public AddCourseDialog(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "Add New Course";
        Width = 70;
        Height = 16;

        var nameLabel = new Label("Course Name:") { X = 1, Y = 1 };
        _nameField = new TextField("")
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(1),
        };

        var codeLabel = new Label("Course Code:") { X = 1, Y = 3 };
        _codeField = new TextField("")
        {
            X = 1,
            Y = 4,
            Width = Dim.Fill(1),
        };

        var ectsLabel = new Label("ECTS Points:") { X = 1, Y = 5 };
        _ectsField = new TextField("5")
        {
            X = 1,
            Y = 6,
            Width = Dim.Fill(1),
        };

        var deptLabel = new Label("Department ID:") { X = 1, Y = 7 };
        _departmentIdField = new TextField("")
        {
            X = 1,
            Y = 8,
            Width = Dim.Fill(1),
        };

        var profLabel = new Label("Professor ID (optional):") { X = 1, Y = 9 };
        _professorIdField = new TextField("")
        {
            X = 1,
            Y = 10,
            Width = Dim.Fill(1),
        };

        var saveButton = new Button("Save")
        {
            X = 1,
            Y = 12,
            IsDefault = true,
        };
        saveButton.Clicked += OnSave;

        var cancelButton = new Button("Cancel") { X = Pos.Right(saveButton) + 2, Y = 12 };
        cancelButton.Clicked += () => TGuiApp.RequestStop();

        Add(
            nameLabel,
            _nameField,
            codeLabel,
            _codeField,
            ectsLabel,
            _ectsField,
            deptLabel,
            _departmentIdField,
            profLabel,
            _professorIdField,
            saveButton,
            cancelButton
        );
    }

    private async void OnSave()
    {
        var name = _nameField.Text.ToString()?.Trim();
        var code = _codeField.Text.ToString()?.Trim();
        var ectsText = _ectsField.Text.ToString()?.Trim();
        var deptIdText = _departmentIdField.Text.ToString()?.Trim();
        var profIdText = _professorIdField.Text.ToString()?.Trim();

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

        if (!int.TryParse(deptIdText, out int deptId))
        {
            MessageBox.ErrorQuery("Validation Error", "Department ID must be a number!", "OK");
            return;
        }

        int? profId = null;
        if (
            !string.IsNullOrWhiteSpace(profIdText) && int.TryParse(profIdText, out int parsedProfId)
        )
        {
            profId = parsedProfId;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();
            await courseService.CreateCourseAsync(name, code, ects, deptId, profId);

            Success = true;
            MessageBox.Query("Success", "Course created successfully!", "OK");
            TGuiApp.RequestStop();
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to create course:\n{ex.Message}", "OK");
        }
    }
}
