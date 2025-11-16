using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Views;

public class DepartmentsView : BaseView
{
    private ListView _listView = null!;
    private List<Department> _departments = new();
    private Button _refreshButton = null!;
    private Label _statusLabel = null!;

    public DepartmentsView(IServiceProvider serviceProvider)
        : base(serviceProvider, "Departments")
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
            _statusLabel.Text = "Loading departments...";
            TGuiApp.MainLoop.Invoke(() => SetNeedsDisplay());

            using var scope = ServiceProvider.CreateScope();
            var departmentService = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
            _departments = (await departmentService.GetAllDepartmentsAsync()).ToList();

            TGuiApp.MainLoop.Invoke(() =>
            {
                var items = _departments.Select(d => $"ID:{d.Id, 4} | {d.Name}").ToList();

                _listView.SetSource(items);
                _statusLabel.Text = $"Total departments: {_departments.Count}";
                SetNeedsDisplay();
            });
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                _statusLabel.Text = "Error loading departments";
                MessageBox.ErrorQuery("Error", $"Failed to load departments:\n{ex.Message}", "OK");
            });
        }
    }
}
