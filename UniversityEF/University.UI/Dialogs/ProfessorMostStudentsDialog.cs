using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Dialogs;

public class ProfessorMostStudentsDialog : Dialog
{
    private readonly IServiceProvider _serviceProvider;
    private Label _resultLabel = null!;
    private Button _btnRun = null!;
    private Button _btnClose = null!;

    public ProfessorMostStudentsDialog(IServiceProvider serviceProvider)
        : base("Professor with Most Students", 70, 15)
    {
        _serviceProvider = serviceProvider;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        var infoLabel = new Label("Click 'Run Query' to find the professor with the most students:")
        {
            X = 2,
            Y = 1,
        };

        _resultLabel = new Label("")
        {
            X = 2,
            Y = 3,
            Width = Dim.Fill(2),
            Height = Dim.Fill(4),
        };

        _btnRun = new Button("Run Query")
        {
            X = Pos.Center() - 10,
            Y = Pos.Bottom(_resultLabel) + 1,
        };
        _btnRun.Clicked += OnRunClicked;

        _btnClose = new Button("Close") { X = Pos.Center() + 5, Y = Pos.Bottom(_resultLabel) + 1 };
        _btnClose.Clicked += () => TGuiApp.RequestStop();

        Add(infoLabel, _resultLabel, _btnRun, _btnClose);
    }

    private async void OnRunClicked()
    {
        try
        {
            _btnRun.Enabled = false;
            _resultLabel.Text = "Running query...";

            using var scope = _serviceProvider.CreateScope();
            var queryService = scope.ServiceProvider.GetRequiredService<IQueryService>();
            var result = await queryService.GetProfessorWithMostStudentsAsync();

            if (result != null)
            {
                _resultLabel.Text =
                    $"Professor: {result.FullName}\n"
                    + $"Index: {result.UniversityIndex}\n"
                    + $"Total Students: {result.TotalStudentCount}\n"
                    + $"Courses Taught: {result.CourseCount}";
            }
            else
            {
                _resultLabel.Text = "No professors found.";
            }
        }
        catch (Exception ex)
        {
            _resultLabel.Text = $"Query failed:\n{ex.Message}";
        }
        finally
        {
            _btnRun.Enabled = true;
        }
    }
}
