using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class AddStudentDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TextField _firstNameField;
    private readonly TextField _lastNameField;
    private readonly TextField _yearField;
    private readonly TextField _streetField;
    private readonly TextField _cityField;
    private readonly TextField _postalCodeField;
    private readonly TextField _prefixField;
    private readonly CheckBox _isMasterCheckBox;
    public bool Success { get; private set; }

    public AddStudentDialog(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "Add New Student";
        Width = 70;
        Height = 22;

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

        var yearLabel = new Label("Year of Study:") { X = 1, Y = 5 };
        _yearField = new TextField("1")
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

        var prefixLabel = new Label("Index Prefix (optional, default: S):") { X = 1, Y = 13 };
        _prefixField = new TextField("S")
        {
            X = 1,
            Y = 14,
            Width = Dim.Fill(1),
        };

        _isMasterCheckBox = new CheckBox("Master Student") { X = 1, Y = 15 };

        var saveButton = new Button("Save")
        {
            X = 1,
            Y = 17,
            IsDefault = true,
        };
        saveButton.Clicked += OnSave;

        var cancelButton = new Button("Cancel") { X = Pos.Right(saveButton) + 2, Y = 17 };
        cancelButton.Clicked += () => TGuiApp.RequestStop();

        Add(
            firstNameLabel,
            _firstNameField,
            lastNameLabel,
            _lastNameField,
            yearLabel,
            _yearField,
            streetLabel,
            _streetField,
            cityLabel,
            _cityField,
            postalLabel,
            _postalCodeField,
            prefixLabel,
            _prefixField,
            _isMasterCheckBox,
            saveButton,
            cancelButton
        );
    }

    private async void OnSave()
    {
        var firstName = _firstNameField.Text.ToString()?.Trim();
        var lastName = _lastNameField.Text.ToString()?.Trim();
        var yearText = _yearField.Text.ToString()?.Trim();
        var street = _streetField.Text.ToString()?.Trim();
        var city = _cityField.Text.ToString()?.Trim();
        var postalCode = _postalCodeField.Text.ToString()?.Trim();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            MessageBox.ErrorQuery("Validation Error", "First and Last name are required!", "OK");
            return;
        }

        if (!int.TryParse(yearText, out int year) || year < 1 || year > 6)
        {
            MessageBox.ErrorQuery("Validation Error", "Year must be between 1 and 6!", "OK");
            return;
        }

        var prefix = _prefixField.Text.ToString()?.Trim()?.ToUpper();
        if (string.IsNullOrWhiteSpace(prefix))
        {
            prefix = "S"; // Default
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();

            var address = new Address
            {
                Street = street ?? "",
                City = city ?? "",
                PostalCode = postalCode ?? "",
            };

            if (_isMasterCheckBox.Checked)
            {
                await studentService.CreateMasterStudentAsync(
                    firstName: firstName,
                    lastName: lastName,
                    yearOfStudy: year,
                    address: address,
                    prefix: prefix,
                    thesisTopic: null,
                    supervisorId: null
                );
            }
            else
            {
                await studentService.CreateStudentAsync(firstName, lastName, year, address, prefix);
            }

            Success = true;
            MessageBox.Query("Success", $"Student created with index prefix '{prefix}'!", "OK");
            TGuiApp.RequestStop();
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to create student:\n{ex.Message}", "OK");
        }
    }
}
