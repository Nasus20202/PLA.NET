using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using University.UI.Dialogs;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Views;

public class StudentsView : BaseView
{
    private ListView _listView = null!;
    private List<Student> _students = new();
    private Button _addButton = null!;
    private Button _editButton = null!;
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
        var hasSelection = args.Item >= 0 && args.Item < _students.Count;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
    }

    private async void OnAddClicked()
    {
        var dialog = new AddStudentDialog(ServiceProvider);
        TGuiApp.Run(dialog);

        if (dialog.Success)
        {
            await LoadDataAsync();
        }
    }

    private async void OnEditClicked()
    {
        if (_listView.SelectedItem < 0 || _listView.SelectedItem >= _students.Count)
            return;

        var student = _students[_listView.SelectedItem];
        var dialog = new UpdateStudentDialog(ServiceProvider, student);
        TGuiApp.Run(dialog);

        if (dialog.Success)
        {
            await LoadDataAsync();
        }
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
                        var fullName = $"{s.FirstName} {s.LastName}";
                        var city = s.ResidenceAddress.City ?? "";
                        var street = s.ResidenceAddress.Street ?? "";
                        return $"ID:{s.Id, 4} | {type, -10} | {s.UniversityIndex, -10} | {fullName, -25} | Y:{s.YearOfStudy} | {city, -20} | {street}";
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
