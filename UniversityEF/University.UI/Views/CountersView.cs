using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Views;

public class CountersView : BaseView
{
    private ListView _listView = null!;
    private List<IndexCounter> _counters = new();
    private Button _refreshButton = null!;
    private Label _statusLabel = null!;

    public CountersView(IServiceProvider serviceProvider)
        : base(serviceProvider, "Index Counters")
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        _statusLabel = new Label("Loading...")
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill(1),
        };

        _listView = new ListView()
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(2),
        };

        var buttonY = Pos.AnchorEnd(1);

        _refreshButton = new Button("Refresh") { X = 1, Y = buttonY };
        _refreshButton.Clicked += async () => await LoadDataAsync();

        Add(_statusLabel, _listView, _refreshButton);
    }

    public override async Task LoadDataAsync()
    {
        try
        {
            _statusLabel.Text = "Loading counters...";
            TGuiApp.MainLoop.Invoke(() => SetNeedsDisplay());

            using var scope = ServiceProvider.CreateScope();
            var counterService = scope.ServiceProvider.GetRequiredService<IIndexCounterService>();
            _counters = (await counterService.GetAllCountersAsync()).ToList();

            TGuiApp.MainLoop.Invoke(() =>
            {
                var items = _counters
                    .Select(c => $"Prefix: {c.Prefix, -10} | Current Value: {c.CurrentValue, 6}")
                    .ToList();

                _listView.SetSource(items);
                _statusLabel.Text = $"Total counters: {_counters.Count}";
                SetNeedsDisplay();
            });
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                _statusLabel.Text = "Error loading counters";
                MessageBox.ErrorQuery("Error", $"Failed to load counters:\n{ex.Message}", "OK");
            });
        }
    }
}
