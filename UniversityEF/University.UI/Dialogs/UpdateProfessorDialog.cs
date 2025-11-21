using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class UpdateProfessorDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private readonly int _professorId;
    private readonly TextField _firstNameField;
    private readonly TextField _lastNameField;
    private readonly TextField _titleField;
    private readonly TextField _streetField;
    private readonly TextField _cityField;
    private readonly TextField _postalCodeField;
    public bool Success { get; private set; }

    public UpdateProfessorDialog(IServiceProvider serviceProvider, Professor professor)
    {
        _serviceProvider = serviceProvider;
        _professorId = professor.Id;
        Title = $"Update Professor (ID: {professor.Id})";
        Width = 70;
        Height = 18;

        var indexLabel = new Label($"Index: {professor.UniversityIndex}") { X = 1, Y = 1 };

        var firstNameLabel = new Label("First Name:") { X = 1, Y = 2 };
        _firstNameField = new TextField(professor.FirstName)
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill(1),
        };

        var lastNameLabel = new Label("Last Name:") { X = 1, Y = 4 };
        _lastNameField = new TextField(professor.LastName)
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill(1),
        };

        var titleLabel = new Label("Academic Title:") { X = 1, Y = 6 };
        _titleField = new TextField(professor.AcademicTitle)
        {
            X = 1,
            Y = 7,
            Width = Dim.Fill(1),
        };

        var streetLabel = new Label("Street:") { X = 1, Y = 8 };
        _streetField = new TextField(professor.ResidenceAddress.Street)
        {
            X = 1,
            Y = 9,
            Width = Dim.Fill(1),
        };

        var cityLabel = new Label("City:") { X = 1, Y = 10 };
        _cityField = new TextField(professor.ResidenceAddress.City)
        {
            X = 1,
            Y = 11,
            Width = Dim.Fill(1),
        };

        var postalLabel = new Label("Postal Code:") { X = 1, Y = 12 };
        _postalCodeField = new TextField(professor.ResidenceAddress.PostalCode)
        {
            X = 1,
            Y = 13,
            Width = Dim.Fill(1),
        };

        var saveButton = new Button("Update")
        {
            X = 1,
            Y = 15,
            IsDefault = true,
        };
        saveButton.Clicked += OnUpdate;

        var cancelButton = new Button("Cancel") { X = Pos.Right(saveButton) + 2, Y = 15 };
        cancelButton.Clicked += () => TGuiApp.RequestStop();

        Add(
            indexLabel,
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

    private async void OnUpdate()
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

            // Get fresh instance from database to avoid tracking conflicts
            var professor = await professorService.GetProfessorByIdAsync(_professorId);
            if (professor == null)
            {
                MessageBox.ErrorQuery("Error", "Professor not found!", "OK");
                return;
            }

            professor.FirstName = firstName;
            professor.LastName = lastName;
            professor.AcademicTitle = title;
            professor.ResidenceAddress.Street = street ?? "";
            professor.ResidenceAddress.City = city ?? "";
            professor.ResidenceAddress.PostalCode = postalCode ?? "";

            await professorService.UpdateProfessorAsync(professor);

            Success = true;
            MessageBox.Query("Success", "Professor updated successfully!", "OK");
            TGuiApp.RequestStop();
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to update professor:\n{ex.Message}", "OK");
        }
    }
}
