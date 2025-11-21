using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using University.UI.Dialogs;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Views;

public class OfficesView : BaseView
{
    private ListView _listView = null!;
    private List<Office> _offices = new();
    private Button _addButton = null!;
    private Button _updateButton = null!;
    private Button _deleteButton = null!;
    private Button _refreshButton = null!;
    private Label _statusLabel = null!;

    public OfficesView(IServiceProvider serviceProvider)
        : base(serviceProvider, "Offices - CRUD Management")
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
            Height = Dim.Fill(3),
        };

        _listView.SelectedItemChanged += OnSelectionChanged;

        // First row of buttons
        var buttonY1 = Pos.AnchorEnd(2);

        _addButton = new Button("Add") { X = 1, Y = buttonY1 };
        _addButton.Clicked += OnAddClicked;

        _updateButton = new Button("Edit")
        {
            X = Pos.Right(_addButton) + 1,
            Y = buttonY1,
            Enabled = false,
        };
        _updateButton.Clicked += OnUpdateClicked;

        _deleteButton = new Button("Delete")
        {
            X = Pos.Right(_updateButton) + 1,
            Y = buttonY1,
            Enabled = false,
        };
        _deleteButton.Clicked += OnDeleteClicked;

        // Second row
        var buttonY2 = Pos.AnchorEnd(1);

        _refreshButton = new Button("Refresh") { X = 1, Y = buttonY2 };
        _refreshButton.Clicked += async () => await LoadDataAsync();

        Add(_statusLabel, _listView, _addButton, _updateButton, _deleteButton, _refreshButton);
    }

    private void OnSelectionChanged(ListViewItemEventArgs args)
    {
        var hasSelection = args.Item >= 0 && args.Item < _offices.Count;
        _updateButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
    }

    private async void OnAddClicked()
    {
        var dialog = new AddOfficeDialog(ServiceProvider);
        TGuiApp.Run(dialog);

        if (dialog.Success)
        {
            await LoadDataAsync();
        }
    }

    private async void OnUpdateClicked()
    {
        if (_listView.SelectedItem < 0 || _listView.SelectedItem >= _offices.Count)
            return;

        var office = _offices[_listView.SelectedItem];
        var dialog = new UpdateOfficeDialog(ServiceProvider, office);
        TGuiApp.Run(dialog);

        if (dialog.Success)
        {
            await LoadDataAsync();
        }
    }

    private async void OnDeleteClicked()
    {
        if (_listView.SelectedItem < 0 || _listView.SelectedItem >= _offices.Count)
            return;

        var office = _offices[_listView.SelectedItem];

        var confirm = MessageBox.Query(
            "Confirm Delete",
            $"Delete office:\n{office.OfficeNumber} in {office.Building}\nProfessor ID: {office.ProfessorId}?\n\nWarning: This cannot be undone!",
            "Yes",
            "No"
        );

        if (confirm == 0)
        {
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var officeService = scope.ServiceProvider.GetRequiredService<IOfficeService>();
                await officeService.DeleteOfficeAsync(office.Id);
                MessageBox.Query("Success", "Office deleted successfully!", "OK");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"Failed to delete office:\n{ex.Message}", "OK");
            }
        }
    }

    public override async Task LoadDataAsync()
    {
        try
        {
            _statusLabel.Text = "Loading offices...";
            TGuiApp.MainLoop.Invoke(() => SetNeedsDisplay());

            using var scope = ServiceProvider.CreateScope();
            var officeService = scope.ServiceProvider.GetRequiredService<IOfficeService>();
            _offices = (await officeService.GetAllOfficesAsync()).ToList();

            TGuiApp.MainLoop.Invoke(() =>
            {
                var items = _offices
                    .Select(o =>
                    {
                        var professorName =
                            o.Professor != null
                                ? $"{o.Professor.FirstName} {o.Professor.LastName}"
                                : "N/A";
                        return $"ID:{o.Id, 4} | Office: {o.OfficeNumber, -10} | Building: {o.Building, -15} | Professor: {professorName}";
                    })
                    .ToList();

                _listView.SetSource(items);
                _statusLabel.Text = $"Total offices: {_offices.Count}";
                _updateButton.Enabled = false;
                _deleteButton.Enabled = false;
                SetNeedsDisplay();
            });
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                _statusLabel.Text = "Error loading offices";
                MessageBox.ErrorQuery("Error", $"Failed to load offices:\n{ex.Message}", "OK");
            });
        }
    }
}
