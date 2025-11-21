using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using University.UI.Dialogs;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Views;

public class ProfessorsView : BaseView
{
    private ListView _listView = null!;
    private List<Professor> _professors = new();
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
    private Button _refreshButton = null!;
    private Label _statusLabel = null!;

    public ProfessorsView(IServiceProvider serviceProvider)
        : base(serviceProvider, "Professors")
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

        _editButton = new Button("Edit")
        {
            X = Pos.Right(_addButton) + 1,
            Y = buttonY1,
            Enabled = false,
        };
        _editButton.Clicked += OnEditClicked;

        _deleteButton = new Button("Delete")
        {
            X = Pos.Right(_editButton) + 1,
            Y = buttonY1,
            Enabled = false,
        };
        _deleteButton.Clicked += OnDeleteClicked;

        // Second row
        var buttonY2 = Pos.AnchorEnd(1);

        _refreshButton = new Button("Refresh") { X = 1, Y = buttonY2 };
        _refreshButton.Clicked += async () => await LoadDataAsync();

        Add(_statusLabel, _listView, _addButton, _editButton, _deleteButton, _refreshButton);
    }

    private void OnSelectionChanged(ListViewItemEventArgs args)
    {
        var hasSelection = args.Item >= 0 && args.Item < _professors.Count;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
    }

    private async void OnAddClicked()
    {
        var dialog = new AddProfessorDialog(ServiceProvider);
        TGuiApp.Run(dialog);

        if (dialog.Success)
        {
            await LoadDataAsync();
        }
    }

    private async void OnEditClicked()
    {
        if (_listView.SelectedItem < 0 || _listView.SelectedItem >= _professors.Count)
            return;

        var professor = _professors[_listView.SelectedItem];
        var dialog = new UpdateProfessorDialog(ServiceProvider, professor);
        TGuiApp.Run(dialog);

        if (dialog.Success)
        {
            await LoadDataAsync();
        }
    }

    private async void OnDeleteClicked()
    {
        if (_listView.SelectedItem < 0 || _listView.SelectedItem >= _professors.Count)
            return;

        var professor = _professors[_listView.SelectedItem];

        var confirm = MessageBox.Query(
            "Confirm Delete",
            $"Delete professor:\n{professor.AcademicTitle} {professor.FirstName} {professor.LastName}\nIndex: {professor.UniversityIndex}?",
            "Yes",
            "No"
        );

        if (confirm == 0)
        {
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var professorService =
                    scope.ServiceProvider.GetRequiredService<IProfessorService>();
                await professorService.DeleteProfessorAsync(professor.Id);
                MessageBox.Query("Success", "Professor deleted successfully!", "OK");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"Failed to delete professor:\n{ex.Message}", "OK");
            }
        }
    }

    public override async Task LoadDataAsync()
    {
        try
        {
            _statusLabel.Text = "Loading professors...";
            TGuiApp.MainLoop.Invoke(() => SetNeedsDisplay());

            using var scope = ServiceProvider.CreateScope();
            var professorService = scope.ServiceProvider.GetRequiredService<IProfessorService>();
            _professors = (await professorService.GetAllProfessorsAsync()).ToList();

            TGuiApp.MainLoop.Invoke(() =>
            {
                var items = _professors
                    .Select(p =>
                    {
                        var fullName = $"{p.AcademicTitle} {p.FirstName} {p.LastName}";
                        var city = p.ResidenceAddress.City ?? "";
                        var street = p.ResidenceAddress.Street ?? "";
                        return $"ID:{p.Id, 4} | {p.UniversityIndex, -10} | {fullName, -30} | {city, -20} | {street}";
                    })
                    .ToList();

                _listView.SetSource(items);
                _statusLabel.Text = $"Total professors: {_professors.Count}";
                _deleteButton.Enabled = false;
                SetNeedsDisplay();
            });
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                _statusLabel.Text = "Error loading professors";
                MessageBox.ErrorQuery("Error", $"Failed to load professors:\n{ex.Message}", "OK");
            });
        }
    }
}
