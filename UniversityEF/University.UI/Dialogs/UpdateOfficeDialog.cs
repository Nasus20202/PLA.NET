using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class UpdateOfficeDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private readonly int _officeId;
    private readonly int _professorId;
    private readonly TextField _officeNumberField;
    private readonly TextField _buildingField;
    public bool Success { get; private set; }

    public UpdateOfficeDialog(IServiceProvider serviceProvider, Office office)
    {
        _serviceProvider = serviceProvider;
        _officeId = office.Id;
        _professorId = office.ProfessorId;
        Title = $"Update Office (ID: {office.Id})";
        Width = 60;
        Height = 14;

        var professorLabel = new Label($"Professor ID: {office.ProfessorId}") { X = 1, Y = 1 };

        var officeNumberLabel = new Label("Office Number:") { X = 1, Y = 3 };
        _officeNumberField = new TextField(office.OfficeNumber)
        {
            X = 1,
            Y = 4,
            Width = Dim.Fill(1),
        };

        var buildingLabel = new Label("Building:") { X = 1, Y = 6 };
        _buildingField = new TextField(office.Building)
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
            professorLabel,
            officeNumberLabel,
            _officeNumberField,
            buildingLabel,
            _buildingField,
            saveButton,
            cancelButton
        );
    }

    private async void OnUpdate()
    {
        var officeNumber = _officeNumberField.Text.ToString()?.Trim();
        var building = _buildingField.Text.ToString()?.Trim();

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

            // Get fresh instance from database to avoid tracking conflicts
            var office = await officeService.GetOfficeByIdAsync(_officeId);
            if (office == null)
            {
                MessageBox.ErrorQuery("Error", "Office not found!", "OK");
                return;
            }

            office.OfficeNumber = officeNumber;
            office.Building = building;

            await officeService.UpdateOfficeAsync(office);

            Success = true;
            MessageBox.Query("Success", $"Office updated to {officeNumber} in {building}!", "OK");
            TGuiApp.RequestStop();
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to update office:\n{ex.Message}", "OK");
        }
    }
}
