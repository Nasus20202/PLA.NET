using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class GenerateDataDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private TextField _profField = null!;
    private TextField _studField = null!;
    private TextField _masterField = null!;
    private Label _progressLabel = null!;
    private Button _btnGenerate = null!;
    private Button _btnCancel = null!;

    public GenerateDataDialog(IServiceProvider serviceProvider)
        : base("Generate Data", 60, 14)
    {
        _serviceProvider = serviceProvider;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        var profLabel = new Label("Professors:") { X = 2, Y = 1 };
        _profField = new TextField("10")
        {
            X = 25,
            Y = 1,
            Width = 10,
        };

        var studLabel = new Label("Bachelor Students:") { X = 2, Y = 3 };
        _studField = new TextField("50")
        {
            X = 25,
            Y = 3,
            Width = 10,
        };

        var masterLabel = new Label("Master Students:") { X = 2, Y = 5 };
        _masterField = new TextField("20")
        {
            X = 25,
            Y = 5,
            Width = 10,
        };

        _progressLabel = new Label("")
        {
            X = 2,
            Y = 7,
            Width = Dim.Fill(2),
        };

        _btnGenerate = new Button("Generate") { X = Pos.Center() - 12, Y = 9 };
        _btnGenerate.Clicked += OnGenerateClicked;

        _btnCancel = new Button("Cancel") { X = Pos.Center() + 3, Y = 9 };
        _btnCancel.Clicked += () => TGuiApp.RequestStop();

        Add(
            profLabel,
            _profField,
            studLabel,
            _studField,
            masterLabel,
            _masterField,
            _progressLabel,
            _btnGenerate,
            _btnCancel
        );
    }

    private async void OnGenerateClicked()
    {
        try
        {
            if (!int.TryParse(_profField.Text.ToString(), out int profCount) || profCount < 0)
            {
                MessageBox.ErrorQuery("Error", "Invalid professor count!", "OK");
                return;
            }

            if (!int.TryParse(_studField.Text.ToString(), out int studCount) || studCount < 0)
            {
                MessageBox.ErrorQuery("Error", "Invalid bachelor student count!", "OK");
                return;
            }

            if (!int.TryParse(_masterField.Text.ToString(), out int masterCount) || masterCount < 0)
            {
                MessageBox.ErrorQuery("Error", "Invalid master student count!", "OK");
                return;
            }

            _btnGenerate.Enabled = false;
            _btnCancel.Enabled = false;
            _progressLabel.Text = "Generating data, please wait...";

            await Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var generator = scope.ServiceProvider.GetRequiredService<IDataGeneratorService>();
                await generator.GenerateDataAsync(profCount, studCount, masterCount);
            });

            TGuiApp.RequestStop();
            MessageBox.Query("Success", "Data generated successfully!", "OK");
        }
        catch (Exception ex)
        {
            _btnGenerate.Enabled = true;
            _btnCancel.Enabled = true;
            _progressLabel.Text = "";
            MessageBox.ErrorQuery("Error", $"Failed to generate data:\n{ex.Message}", "OK");
        }
    }
}
