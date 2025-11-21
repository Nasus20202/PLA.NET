using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class AddProfessorDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TextField _firstNameField;
    private readonly TextField _lastNameField;
    private readonly TextField _titleField;
    private readonly TextField _streetField;
    private readonly TextField _cityField;
    private readonly TextField _postalCodeField;
    public bool Success { get; private set; }

    public AddProfessorDialog(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "Add New Professor";
        Width = 70;
        Height = 18;

        var firstNameLabel = new Label("First Name:") { X = 1, Y = 1 };
        _firstNameField = new TextField("")
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(1),
        };

        var lastNameLabel = new Label("Last Name:") { X = 1, Y = 3 };
        _lastNameField = new TextField("")
        {
            X = 1,
            Y = 4,
            Width = Dim.Fill(1),
        };

        var titleLabel = new Label("Academic Title (e.g., Dr, Prof):") { X = 1, Y = 5 };
        _titleField = new TextField("Dr")
        {
            X = 1,
            Y = 6,
            Width = Dim.Fill(1),
        };

        var streetLabel = new Label("Street:") { X = 1, Y = 7 };
        _streetField = new TextField("")
        {
            X = 1,
            Y = 8,
            Width = Dim.Fill(1),
        };

        var cityLabel = new Label("City:") { X = 1, Y = 9 };
        _cityField = new TextField("")
        {
            X = 1,
            Y = 10,
            Width = Dim.Fill(1),
        };

        var postalLabel = new Label("Postal Code:") { X = 1, Y = 11 };
        _postalCodeField = new TextField("")
        {
            X = 1,
            Y = 12,
            Width = Dim.Fill(1),
        };

        var saveButton = new Button("Save")
        {
            X = 1,
            Y = 14,
            IsDefault = true,
        };
        saveButton.Clicked += OnSave;

        var cancelButton = new Button("Cancel") { X = Pos.Right(saveButton) + 2, Y = 14 };
        cancelButton.Clicked += () => TGuiApp.RequestStop();

        Add(
            firstNameLabel,
            _firstNameField,
            lastNameLabel,
            _lastNameField,
            titleLabel,
            _titleField,
            streetLabel,
            _streetField,
            cityLabel,
            _cityField,
            postalLabel,
            _postalCodeField,
            saveButton,
            cancelButton
        );
    }

    private async void OnSave()
    {
        var firstName = _firstNameField.Text.ToString()?.Trim();
        var lastName = _lastNameField.Text.ToString()?.Trim();
        var title = _titleField.Text.ToString()?.Trim();
        var street = _streetField.Text.ToString()?.Trim();
        var city = _cityField.Text.ToString()?.Trim();
        var postalCode = _postalCodeField.Text.ToString()?.Trim();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            MessageBox.ErrorQuery("Validation Error", "First and Last name are required!", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.ErrorQuery("Validation Error", "Academic title is required!", "OK");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var professorService = scope.ServiceProvider.GetRequiredService<IProfessorService>();

            var address = new Address
            {
                Street = street ?? "",
                City = city ?? "",
                PostalCode = postalCode ?? "",
            };

            await professorService.CreateProfessorAsync(firstName, lastName, title, address);

            Success = true;
            MessageBox.Query("Success", "Professor created successfully!", "OK");
            TGuiApp.RequestStop();
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to create professor:\n{ex.Message}", "OK");
        }
    }
}
