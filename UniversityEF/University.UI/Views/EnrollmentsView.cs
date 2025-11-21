using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Domain.Entities;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI.Views;

public class EnrollmentsView : BaseView
{
    private ListView _listView = null!;
    private List<Enrollment> _enrollments = new();
    private Button _addButton = null!;
    private Button _deleteButton = null!;
    private Button _refreshButton = null!;
    private Label _statusLabel = null!;

    public EnrollmentsView(IServiceProvider serviceProvider)
        : base(serviceProvider, "Student Enrollments")
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

        _addButton = new Button("Enroll Student") { X = 1, Y = buttonY1 };
        _addButton.Clicked += OnAddClicked;

        _deleteButton = new Button("Remove Enrollment")
        {
            X = Pos.Right(_addButton) + 1,
            Y = buttonY1,
            Enabled = false,
        };
        _deleteButton.Clicked += OnDeleteClicked;

        // Second row
        var buttonY2 = Pos.AnchorEnd(1);

        _refreshButton = new Button("Refresh") { X = 1, Y = buttonY2 };
        _refreshButton.Clicked += async () => await LoadDataAsync();

        Add(_statusLabel, _listView, _addButton, _deleteButton, _refreshButton);
    }

    private void OnSelectionChanged(ListViewItemEventArgs args)
    {
        _deleteButton.Enabled = args.Item >= 0 && args.Item < _enrollments.Count;
    }

    private async void OnAddClicked()
    {
        // Simple dialog to enroll a student in a course
        var dialog = new Dialog("Enroll Student in Course", 70, 16);

        var studentIdLabel = new Label("Student ID:") { X = 1, Y = 1 };
        var studentIdField = new TextField("")
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(1),
        };

        var courseIdLabel = new Label("Course ID:") { X = 1, Y = 4 };
        var courseIdField = new TextField("")
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill(1),
        };

        var semesterLabel = new Label("Semester:") { X = 1, Y = 7 };
        var semesterField = new TextField("")
        {
            X = 1,
            Y = 8,
            Width = Dim.Fill(1),
        };

        var enrollButton = new Button("Enroll")
        {
            X = 1,
            Y = 10,
            IsDefault = true,
        };
        var cancelButton = new Button("Cancel") { X = Pos.Right(enrollButton) + 2, Y = 10 };

        bool success = false;
        enrollButton.Clicked += async () =>
        {
            if (
                !int.TryParse(studentIdField.Text.ToString(), out int studentId)
                || !int.TryParse(courseIdField.Text.ToString(), out int courseId)
            )
            {
                MessageBox.ErrorQuery("Error", "Invalid Student ID or Course ID!", "OK");
                return;
            }

            var semester = semesterField.Text.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(semester))
            {
                MessageBox.ErrorQuery("Error", "Semester is required!", "OK");
                return;
            }

            try
            {
                using var scope = ServiceProvider.CreateScope();
                var enrollmentService =
                    scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
                await enrollmentService.EnrollStudentAsync(studentId, courseId, semester);

                success = true;
                MessageBox.Query("Success", "Student enrolled successfully!", "OK");
                TGuiApp.RequestStop();
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"Failed to enroll:\n{ex.Message}", "OK");
            }
        };

        cancelButton.Clicked += () => TGuiApp.RequestStop();

        dialog.Add(
            studentIdLabel,
            studentIdField,
            courseIdLabel,
            courseIdField,
            semesterLabel,
            semesterField,
            enrollButton,
            cancelButton
        );

        TGuiApp.Run(dialog);

        if (success)
        {
            await LoadDataAsync();
        }
    }

    private async void OnDeleteClicked()
    {
        if (_listView.SelectedItem < 0 || _listView.SelectedItem >= _enrollments.Count)
            return;

        var enrollment = _enrollments[_listView.SelectedItem];

        var confirm = MessageBox.Query(
            "Confirm Delete",
            $"Remove enrollment:\nStudent ID: {enrollment.StudentId}\nCourse ID: {enrollment.CourseId}\nSemester: {enrollment.Semester}?",
            "Yes",
            "No"
        );

        if (confirm == 0)
        {
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var enrollmentService =
                    scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
                await enrollmentService.UnenrollStudentAsync(enrollment.Id);

                MessageBox.Query("Success", "Enrollment removed successfully!", "OK");
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"Failed to remove enrollment:\n{ex.Message}", "OK");
            }
        }
    }

    public override async Task LoadDataAsync()
    {
        try
        {
            _statusLabel.Text = "Loading enrollments...";
            TGuiApp.MainLoop.Invoke(() => SetNeedsDisplay());

            using var scope = ServiceProvider.CreateScope();
            var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
            _enrollments = (await enrollmentService.GetAllEnrollmentsAsync()).ToList();

            TGuiApp.MainLoop.Invoke(() =>
            {
                var items = _enrollments
                    .Select(e =>
                    {
                        var grade = e.Grade.HasValue ? e.Grade.Value.ToString("F1") : "N/A";
                        var studentName =
                            e.Student != null
                                ? $"{e.Student.FirstName} {e.Student.LastName}"
                                : "Unknown";
                        var courseName = e.Course != null ? e.Course.Name : "Unknown";
                        return $"ID:{e.Id, 4} | Student: {studentName, -25} | Course: {courseName, -30} | Sem: {e.Semester, -10} | Grade: {grade}";
                    })
                    .ToList();

                _listView.SetSource(items);
                _statusLabel.Text = $"Total enrollments: {_enrollments.Count}";
                _deleteButton.Enabled = false;
                SetNeedsDisplay();
            });
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                _statusLabel.Text = "Error loading enrollments";
                MessageBox.ErrorQuery("Error", $"Failed to load enrollments:\n{ex.Message}", "OK");
            });
        }
    }
}
