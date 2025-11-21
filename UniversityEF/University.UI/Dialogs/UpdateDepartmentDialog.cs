using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class UpdateDepartmentDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private readonly int _departmentId;
    private readonly TextField _nameField;
    public bool Success { get; private set; }

    public UpdateDepartmentDialog(IServiceProvider serviceProvider, Department department)
    {
        _serviceProvider = serviceProvider;
        _departmentId = department.Id;
        Title = $"Update Department (ID: {department.Id})";
        Width = 60;
        Height = 10;

        var nameLabel = new Label("Department Name:") { X = 1, Y = 1 };
        _nameField = new TextField(department.Name)
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(1),
        };

        var saveButton = new Button("Update")
        {
            X = 1,
            Y = 4,
            IsDefault = true,
        };
        saveButton.Clicked += OnUpdate;

        var cancelButton = new Button("Cancel") { X = Pos.Right(saveButton) + 2, Y = 4 };
        cancelButton.Clicked += () => TGuiApp.RequestStop();

        Add(nameLabel, _nameField, saveButton, cancelButton);
    }

    private async void OnUpdate()
    {
        var name = _nameField.Text.ToString()?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.ErrorQuery("Validation Error", "Department name is required!", "OK");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var departmentService = scope.ServiceProvider.GetRequiredService<IDepartmentService>();

            // Get fresh instance from database to avoid tracking conflicts
            var department = await departmentService.GetDepartmentByIdAsync(_departmentId);
            if (department == null)
            {
                MessageBox.ErrorQuery("Error", "Department not found!", "OK");
                return;
            }

            department.Name = name;

            await departmentService.UpdateDepartmentAsync(department);

            Success = true;
            MessageBox.Query("Success", "Department updated successfully!", "OK");
            TGuiApp.RequestStop();
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to update department:\n{ex.Message}", "OK");
        }
    }
}
