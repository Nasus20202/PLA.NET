using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class UpdateCounterDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _prefix;
    private readonly TextField _newValueField;
    public bool Success { get; private set; }

    public UpdateCounterDialog(IServiceProvider serviceProvider, IndexCounter counter)
    {
        _serviceProvider = serviceProvider;
        _prefix = counter.Prefix;
        Title = $"Update Counter: {counter.Prefix}";
        Width = 60;
        Height = 12;

        var prefixLabel = new Label($"Prefix: {counter.Prefix}") { X = 1, Y = 1 };
        var currentValueLabel = new Label($"Current Value: {counter.CurrentValue}")
        {
            X = 1,
            Y = 2,
        };

        var newValueLabel = new Label("New Value:") { X = 1, Y = 4 };
        _newValueField = new TextField(counter.CurrentValue.ToString())
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill(1),
        };

        var saveButton = new Button("Update")
        {
            X = 1,
            Y = 7,
            IsDefault = true,
        };
        saveButton.Clicked += OnUpdate;

        var cancelButton = new Button("Cancel") { X = Pos.Right(saveButton) + 2, Y = 7 };
        cancelButton.Clicked += () => TGuiApp.RequestStop();

        Add(
            prefixLabel,
            currentValueLabel,
            newValueLabel,
            _newValueField,
            saveButton,
            cancelButton
        );
    }

    private async void OnUpdate()
    {
        var newValueText = _newValueField.Text.ToString()?.Trim();

        if (!int.TryParse(newValueText, out int newValue))
        {
            MessageBox.ErrorQuery("Validation Error", "New value must be a valid number!", "OK");
            return;
        }

        if (newValue < 0)
        {
            MessageBox.ErrorQuery("Validation Error", "Value cannot be negative!", "OK");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var counterService = scope.ServiceProvider.GetRequiredService<IIndexCounterService>();
            await counterService.UpdateCounterAsync(_prefix, newValue);

            Success = true;
            MessageBox.Query("Success", $"Counter '{_prefix}' updated to {newValue}!", "OK");
            TGuiApp.RequestStop();
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Failed to update counter:\n{ex.Message}", "OK");
        }
    }
}
