using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class AddCounterDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TextField _prefixField;
    private readonly TextField _startValueField;
    public bool Success { get; private set; }

    public AddCounterDialog(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "Add New Index Counter";
        Width = 60;
        Height = 12;

        var prefixLabel = new Label("Prefix (e.g., D for Doctorate):") { X = 1, Y = 1 };
        _prefixField = new TextField("")
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(1),
        };

        var startValueLabel = new Label("Start Value:") { X = 1, Y = 4 };
        _startValueField = new TextField("0")
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill(1),
        };

        var saveButton = new Button("Save")
        {
            X = 1,
            Y = 7,
            IsDefault = true,
        };
        saveButton.Clicked += OnSave;

        var cancelButton = new Button("Cancel") { X = Pos.Right(saveButton) + 2, Y = 7 };
        cancelButton.Clicked += () => TGuiApp.RequestStop();

        Add(prefixLabel, _prefixField, startValueLabel, _startValueField, saveButton, cancelButton);
    }

    private async void OnSave()
    {
        var prefix = _prefixField.Text.ToString()?.Trim();
        var startValueText = _startValueField.Text.ToString()?.Trim();

        if (string.IsNullOrWhiteSpace(prefix))
        {
            MessageBox.ErrorQuery("Validation Error", "Prefix cannot be empty!", "OK");
            return;
        }

        if (!int.TryParse(startValueText, out int startValue))
        {
            MessageBox.ErrorQuery("Validation Error", "Start value must be a valid number!", "OK");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var counterService = scope.ServiceProvider.GetRequiredService<IIndexCounterService>();
            await counterService.InitializeCounterAsync(prefix, startValue);

            Success = true;
            MessageBox.Query("Success", $"Counter '{prefix}' created successfully!", "OK");
            TGuiApp.RequestStop();
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to create counter:\n{ex.Message}", "OK");
        }
    }
}
