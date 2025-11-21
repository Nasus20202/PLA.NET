using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class UpdateStudentDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private readonly int _studentId;
    private readonly TextField _firstNameField;
    private readonly TextField _lastNameField;
    private readonly TextField _yearField;
    private readonly TextField _streetField;
    private readonly TextField _cityField;
    private readonly TextField _postalCodeField;
    public bool Success { get; private set; }

    public UpdateStudentDialog(IServiceProvider serviceProvider, Student student)
    {
        _serviceProvider = serviceProvider;
        _studentId = student.Id;
        Title = $"Update Student (ID: {student.Id})";
        Width = 70;
        Height = 18;

        var indexLabel = new Label($"Index: {student.UniversityIndex}") { X = 1, Y = 1 };

        var firstNameLabel = new Label("First Name:") { X = 1, Y = 2 };
        _firstNameField = new TextField(student.FirstName)
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill(1),
        };

        var lastNameLabel = new Label("Last Name:") { X = 1, Y = 4 };
        _lastNameField = new TextField(student.LastName)
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill(1),
        };

        var yearLabel = new Label("Year of Study:") { X = 1, Y = 6 };
        _yearField = new TextField(student.YearOfStudy.ToString())
        {
            X = 1,
            Y = 7,
            Width = Dim.Fill(1),
        };

        var streetLabel = new Label("Street:") { X = 1, Y = 8 };
        _streetField = new TextField(student.ResidenceAddress.Street)
        {
            X = 1,
            Y = 9,
            Width = Dim.Fill(1),
        };

        var cityLabel = new Label("City:") { X = 1, Y = 10 };
        _cityField = new TextField(student.ResidenceAddress.City)
        {
            X = 1,
            Y = 11,
            Width = Dim.Fill(1),
        };

        var postalLabel = new Label("Postal Code:") { X = 1, Y = 12 };
        _postalCodeField = new TextField(student.ResidenceAddress.PostalCode)
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
            yearLabel,
            _yearField,
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

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();

            // Get fresh instance from database to avoid tracking conflicts
            var student = await studentService.GetStudentByIdAsync(_studentId);
            if (student == null)
            {
                MessageBox.ErrorQuery("Error", "Student not found!", "OK");
                return;
            }

            student.FirstName = firstName;
            student.LastName = lastName;
            student.YearOfStudy = year;
            student.ResidenceAddress.Street = street ?? "";
            student.ResidenceAddress.City = city ?? "";
            student.ResidenceAddress.PostalCode = postalCode ?? "";

            await studentService.UpdateStudentAsync(student);

            Success = true;
            MessageBox.Query("Success", "Student updated successfully!", "OK");
            TGuiApp.RequestStop();
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to update student:\n{ex.Message}", "OK");
        }
    }
}
