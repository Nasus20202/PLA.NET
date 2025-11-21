using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class AddOfficeDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TextField _professorIdField;
    private readonly TextField _officeNumberField;
    private readonly TextField _buildingField;
    public bool Success { get; private set; }

    public AddOfficeDialog(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "Add New Office";
        Width = 60;
        Height = 16;

        var professorIdLabel = new Label("Professor ID:") { X = 1, Y = 1 };
        _professorIdField = new TextField("")
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(1),
        };

        var officeNumberLabel = new Label("Office Number:") { X = 1, Y = 4 };
        _officeNumberField = new TextField("")
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill(1),
        };

        var buildingLabel = new Label("Building:") { X = 1, Y = 7 };
        _buildingField = new TextField("")
        {
            X = 1,
            Y = 8,
            Width = Dim.Fill(1),
        };

        var saveButton = new Button("Save")
        {
            X = 1,
            Y = 10,
            IsDefault = true,
        };
        saveButton.Clicked += OnSave;

        var cancelButton = new Button("Cancel") { X = Pos.Right(saveButton) + 2, Y = 10 };
        cancelButton.Clicked += () => TGuiApp.RequestStop();

        Add(
            professorIdLabel,
            _professorIdField,
            officeNumberLabel,
            _officeNumberField,
            buildingLabel,
            _buildingField,
            saveButton,
            cancelButton
        );
    }

    private async void OnSave()
    {
        var professorIdText = _professorIdField.Text.ToString()?.Trim();
        var officeNumber = _officeNumberField.Text.ToString()?.Trim();
        var building = _buildingField.Text.ToString()?.Trim();

        if (!int.TryParse(professorIdText, out int professorId))
        {
            MessageBox.ErrorQuery("Validation Error", "Professor ID must be a valid number!", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(officeNumber))
        {
            MessageBox.ErrorQuery("Validation Error", "Office number cannot be empty!", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(building))
        {
            MessageBox.ErrorQuery("Validation Error", "Building cannot be empty!", "OK");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var officeService = scope.ServiceProvider.GetRequiredService<IOfficeService>();
            await officeService.CreateOfficeAsync(professorId, officeNumber, building);

            Success = true;
            MessageBox.Query(
                "Success",
                $"Office {officeNumber} in {building} created successfully!",
                "OK"
            );
            TGuiApp.RequestStop();
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to create office:\n{ex.Message}", "OK");
        }
    }
}
