using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Views;

public class ProfessorsView : BaseView
{
    private ListView _listView = null!;
    private List<Professor> _professors = new();
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
            Height = Dim.Fill(2),
        };

        _listView.SelectedItemChanged += OnSelectionChanged;

        var buttonY = Pos.AnchorEnd(1);

        _refreshButton = new Button("Refresh") { X = 1, Y = buttonY };
        _refreshButton.Clicked += async () => await LoadDataAsync();

        _deleteButton = new Button("Delete Selected")
        {
            X = Pos.Right(_refreshButton) + 2,
            Y = buttonY,
            Enabled = false,
        };
        _deleteButton.Clicked += OnDeleteClicked;

        Add(_statusLabel, _listView, _refreshButton, _deleteButton);
    }

    private void OnSelectionChanged(ListViewItemEventArgs args)
    {
        _deleteButton.Enabled = args.Item >= 0 && args.Item < _professors.Count;
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
                        $"ID:{p.Id, 4} | {p.UniversityIndex, -10} | {p.AcademicTitle} {p.FirstName} {p.LastName}"
                    )
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
