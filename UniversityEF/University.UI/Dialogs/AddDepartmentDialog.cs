using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class AddDepartmentDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TextField _nameField;
    public bool Success { get; private set; }

    public AddDepartmentDialog(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "Add New Department";
        Width = 60;
        Height = 10;

        var nameLabel = new Label("Department Name:") { X = 1, Y = 1 };
        _nameField = new TextField("")
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(1),
        };

        var saveButton = new Button("Save")
        {
            X = 1,
            Y = 4,
            IsDefault = true,
        };
        saveButton.Clicked += OnSave;

        var cancelButton = new Button("Cancel") { X = Pos.Right(saveButton) + 2, Y = 4 };
        cancelButton.Clicked += () => TGuiApp.RequestStop();

        Add(nameLabel, _nameField, saveButton, cancelButton);
    }

    private async void OnSave()
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
            await departmentService.CreateDepartmentAsync(name);

            Success = true;
            MessageBox.Query("Success", "Department created successfully!", "OK");
            TGuiApp.RequestStop();
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to create department:\n{ex.Message}", "OK");
        }
    }
}
