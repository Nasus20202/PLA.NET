using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Views;

public class StudentsView : BaseView
{
    private ListView _listView = null!;
    private List<Student> _students = new();
    private Button _deleteButton = null!;
    private Button _refreshButton = null!;
    private Label _statusLabel = null!;

    public StudentsView(IServiceProvider serviceProvider)
        : base(serviceProvider, "Students")
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
        _deleteButton.Enabled = args.Item >= 0 && args.Item < _students.Count;
    }

    private async void OnDeleteClicked()
    {
        if (_listView.SelectedItem < 0 || _listView.SelectedItem >= _students.Count)
            return;

        var student = _students[_listView.SelectedItem];
        var type = student is MasterStudent ? "Master" : "Bachelor";

        var confirm = MessageBox.Query(
            "Confirm Delete",
            $"Delete {type} student:\n{student.FirstName} {student.LastName}\nIndex: {student.UniversityIndex}?",
            "Yes",
            "No"
        );

        if (confirm == 0)
        {
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();
                await studentService.DeleteStudentAsync(student.Id);
                MessageBox.Query("Success", "Student deleted successfully!", "OK");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"Failed to delete student:\n{ex.Message}", "OK");
            }
        }
    }

    public override async Task LoadDataAsync()
    {
        try
        {
            _statusLabel.Text = "Loading students...";
            TGuiApp.MainLoop.Invoke(() => SetNeedsDisplay());

            using var scope = ServiceProvider.CreateScope();
            var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();
            _students = (await studentService.GetAllStudentsAsync()).ToList();

            TGuiApp.MainLoop.Invoke(() =>
            {
                var items = _students
                    .Select(s =>
                    {
                        var type = s is MasterStudent ? "[Master]" : "[Bachelor]";
                        return $"ID:{s.Id, 4} | {type, -10} | {s.UniversityIndex, -10} | {s.FirstName} {s.LastName} | Year {s.YearOfStudy}";
                    })
                    .ToList();

                _listView.SetSource(items);
                _statusLabel.Text = $"Total students: {_students.Count}";
                _deleteButton.Enabled = false;
                SetNeedsDisplay();
            });
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                _statusLabel.Text = "Error loading students";
                MessageBox.ErrorQuery("Error", $"Failed to load students:\n{ex.Message}", "OK");
            });
        }
    }
}
