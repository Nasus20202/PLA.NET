using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.Application.Interfaces;
using University.Application.Services;
using University.Infrastructure.Data;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI;

class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<UniversityDbContext>();
            await context.Database.EnsureCreatedAsync();
        }

        TGuiApp.Init();
        try
        {
            var app = new UniversityTUI(serviceProvider);
            TGuiApp.Run(app);
        }
        finally
        {
            TGuiApp.Shutdown();
        }
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddDbContext<UniversityDbContext>(options =>
            options.UseSqlite("Data Source=university.db")
        );

        services.AddScoped<IUniversityRepository, UniversityRepository>();
        services.AddScoped<IIndexCounterService, IndexCounterService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IProfessorService, ProfessorService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IQueryService, QueryService>();
        services.AddScoped<DataGeneratorService>();
    }
}

class UniversityTUI : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly FrameView _contentFrame;

    public UniversityTUI(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "University Management System - EF Core (Press F10 to quit)";

        var menu = new MenuBar(
            new MenuBarItem[]
            {
                new MenuBarItem(
                    "_File",
                    new MenuItem[]
                    {
                        new MenuItem("_Generate Data", "", () => ShowGenerateDataDialog()),
                        new MenuItem("_Quit", "", () => TGuiApp.RequestStop()),
                    }
                ),
                new MenuBarItem(
                    "_Browse",
                    new MenuItem[]
                    {
                        new MenuItem("_Students", "", () => Task.Run(ShowStudentsAsync).Wait()),
                        new MenuItem("_Professors", "", () => Task.Run(ShowProfessorsAsync).Wait()),
                        new MenuItem("_Courses", "", () => Task.Run(ShowCoursesAsync).Wait()),
                        new MenuItem(
                            "_Departments",
                            "",
                            () => Task.Run(ShowDepartmentsAsync).Wait()
                        ),
                        new MenuItem("C_ounters", "", () => Task.Run(ShowCountersAsync).Wait()),
                    }
                ),
                new MenuBarItem(
                    "_Manage",
                    new MenuItem[]
                    {
                        new MenuItem("Delete _Student", "", () => ShowDeleteStudentDialog()),
                        new MenuItem("Delete _Professor", "", () => ShowDeleteProfessorDialog()),
                    }
                ),
                new MenuBarItem(
                    "_Queries",
                    new MenuItem[]
                    {
                        new MenuItem(
                            "Professor with _Most Students",
                            "",
                            () => Task.Run(RunQueryProfessorMostStudentsAsync).Wait()
                        ),
                        new MenuItem(
                            "Course _Averages",
                            "",
                            () => Task.Run(RunQueryCourseAveragesAsync).Wait()
                        ),
                        new MenuItem(
                            "Student _Difficulty",
                            "",
                            () => Task.Run(RunQueryStudentDifficultyAsync).Wait()
                        ),
                    }
                ),
            }
        );

        _contentFrame = new FrameView("Content")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
        };

        var statusBar = new StatusBar(
            new StatusItem[] { new StatusItem(Key.F10, "~F10~ Quit", () => TGuiApp.RequestStop()) }
        );

        Add(menu, _contentFrame, statusBar);
        ShowWelcomeScreen();
    }

    private void ShowWelcomeScreen()
    {
        _contentFrame.RemoveAll();
        var welcome = new Label()
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Text =
                "Welcome to University Management System\n\n"
                + "Built with Entity Framework Core\n"
                + "Clean Architecture + Repository Pattern\n\n"
                + "Use the menu bar above to navigate",
            TextAlignment = TextAlignment.Centered,
        };
        _contentFrame.Add(welcome);
    }

    private void ShowGenerateDataDialog()
    {
        var dialog = new Dialog("Generate Data", 60, 14);

        var profLabel = new Label("Professors:") { X = 2, Y = 1 };
        var profField = new TextField("10")
        {
            X = 25,
            Y = 1,
            Width = 10,
        };

        var studLabel = new Label("Bachelor Students:") { X = 2, Y = 3 };
        var studField = new TextField("50")
        {
            X = 25,
            Y = 3,
            Width = 10,
        };

        var masterLabel = new Label("Master Students:") { X = 2, Y = 5 };
        var masterField = new TextField("20")
        {
            X = 25,
            Y = 5,
            Width = 10,
        };

        var progressLabel = new Label("")
        {
            X = 2,
            Y = 7,
            Width = Dim.Fill(2),
        };

        var btnGenerate = new Button("Generate") { X = Pos.Center() - 12, Y = 9 };
        var btnCancel = new Button("Cancel") { X = Pos.Center() + 3, Y = 9 };

        btnGenerate.Clicked += async () =>
        {
            try
            {
                var profCount = int.Parse(profField.Text.ToString() ?? "10");
                var studCount = int.Parse(studField.Text.ToString() ?? "50");
                var masterCount = int.Parse(masterField.Text.ToString() ?? "20");

                btnGenerate.Enabled = false;
                btnCancel.Enabled = false;
                progressLabel.Text = "Generating data, please wait...";

                await Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var generator =
                        scope.ServiceProvider.GetRequiredService<DataGeneratorService>();
                    await generator.GenerateDataAsync(profCount, studCount, masterCount);
                });

                TGuiApp.RequestStop();
                MessageBox.Query("Success", "Data generated successfully!", "OK");
            }
            catch (Exception ex)
            {
                TGuiApp.RequestStop();
                MessageBox.ErrorQuery("Error", $"Failed to generate data:\n{ex.Message}", "OK");
            }
        };

        btnCancel.Clicked += () => TGuiApp.RequestStop();

        dialog.Add(
            profLabel,
            profField,
            studLabel,
            studField,
            masterLabel,
            masterField,
            progressLabel,
            btnGenerate,
            btnCancel
        );
        TGuiApp.Run(dialog);
    }

    private async Task ShowStudentsAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();
            var students = await studentService.GetAllStudentsAsync();

            TGuiApp.MainLoop.Invoke(() =>
            {
                _contentFrame.RemoveAll();

                var listView = new ListView(
                    students
                        .Select(s =>
                        {
                            var type = s is Domain.Entities.MasterStudent ? "[M]" : "[B]";
                            return $"{type} #{s.Id} {s.UniversityIndex} - {s.FirstName} {s.LastName} (Year {s.YearOfStudy})";
                        })
                        .ToList()
                )
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill(),
                };

                _contentFrame.Add(listView);
            });
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                MessageBox.ErrorQuery("Error", $"Failed to load students:\n{ex.Message}", "OK");
            });
        }
    }

    private async Task ShowProfessorsAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var professorService = scope.ServiceProvider.GetRequiredService<IProfessorService>();
            var professors = await professorService.GetAllProfessorsAsync();

            TGuiApp.MainLoop.Invoke(() =>
            {
                _contentFrame.RemoveAll();

                var listView = new ListView(
                    professors
                        .Select(p =>
                            $"#{p.Id} {p.UniversityIndex} - {p.AcademicTitle} {p.FirstName} {p.LastName}"
                        )
                        .ToList()
                )
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill(),
                };

                _contentFrame.Add(listView);
            });
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                MessageBox.ErrorQuery("Error", $"Failed to load professors:\n{ex.Message}", "OK");
            });
        }
    }

    private async Task ShowCoursesAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();
            var courses = await courseService.GetAllCoursesAsync();

            TGuiApp.MainLoop.Invoke(() =>
            {
                _contentFrame.RemoveAll();

                var listView = new ListView(
                    courses
                        .Select(c =>
                            $"#{c.Id} {c.CourseCode} - {c.Name} ({c.ECTSPoints} ECTS) - Dept: {c.Department.Name}"
                        )
                        .ToList()
                )
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill(),
                };

                _contentFrame.Add(listView);
            });
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                MessageBox.ErrorQuery("Error", $"Failed to load courses:\n{ex.Message}", "OK");
            });
        }
    }

    private async Task ShowDepartmentsAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var departmentService = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
            var departments = await departmentService.GetAllDepartmentsAsync();

            TGuiApp.MainLoop.Invoke(() =>
            {
                _contentFrame.RemoveAll();

                var listView = new ListView(departments.Select(d => $"#{d.Id} - {d.Name}").ToList())
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill(),
                };

                _contentFrame.Add(listView);
            });
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                MessageBox.ErrorQuery("Error", $"Failed to load departments:\n{ex.Message}", "OK");
            });
        }
    }

    private async Task ShowCountersAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var counterService = scope.ServiceProvider.GetRequiredService<IIndexCounterService>();
            var counters = await counterService.GetAllCountersAsync();

            TGuiApp.MainLoop.Invoke(() =>
            {
                _contentFrame.RemoveAll();

                var listView = new ListView(
                    counters
                        .Select(c => $"Prefix: {c.Prefix} -> Current Value: {c.CurrentValue}")
                        .ToList()
                )
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill(),
                };

                _contentFrame.Add(listView);
            });
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                MessageBox.ErrorQuery("Error", $"Failed to load counters:\n{ex.Message}", "OK");
            });
        }
    }

    private void ShowDeleteStudentDialog()
    {
        var dialog = new Dialog("Delete Student", 50, 10);
        var label = new Label("Student ID:") { X = 2, Y = 1 };
        var idField = new TextField("")
        {
            X = 15,
            Y = 1,
            Width = 20,
        };
        var btnDelete = new Button("Delete") { X = Pos.Center() - 12, Y = 4 };
        var btnCancel = new Button("Cancel") { X = Pos.Center() + 3, Y = 4 };

        btnDelete.Clicked += async () =>
        {
            try
            {
                if (!int.TryParse(idField.Text.ToString(), out int id))
                {
                    MessageBox.ErrorQuery("Error", "Invalid ID format!", "OK");
                    return;
                }

                TGuiApp.RequestStop();

                using var scope = _serviceProvider.CreateScope();
                var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();
                var student = await studentService.GetStudentByIdAsync(id);

                if (student == null)
                {
                    MessageBox.ErrorQuery("Error", "Student not found!", "OK");
                    return;
                }

                var confirm = MessageBox.Query(
                    "Confirm Delete",
                    $"Delete student:\n{student.FirstName} {student.LastName} ({student.UniversityIndex})?",
                    "Yes",
                    "No"
                );

                if (confirm == 0)
                {
                    await studentService.DeleteStudentAsync(id);
                    MessageBox.Query("Success", "Student deleted successfully!", "OK");
                }
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"Failed to delete student:\n{ex.Message}", "OK");
            }
        };

        btnCancel.Clicked += () => TGuiApp.RequestStop();
        dialog.Add(label, idField, btnDelete, btnCancel);
        TGuiApp.Run(dialog);
    }

    private void ShowDeleteProfessorDialog()
    {
        var dialog = new Dialog("Delete Professor", 50, 10);
        var label = new Label("Professor ID:") { X = 2, Y = 1 };
        var idField = new TextField("")
        {
            X = 17,
            Y = 1,
            Width = 20,
        };
        var btnDelete = new Button("Delete") { X = Pos.Center() - 12, Y = 4 };
        var btnCancel = new Button("Cancel") { X = Pos.Center() + 3, Y = 4 };

        btnDelete.Clicked += async () =>
        {
            try
            {
                if (!int.TryParse(idField.Text.ToString(), out int id))
                {
                    MessageBox.ErrorQuery("Error", "Invalid ID format!", "OK");
                    return;
                }

                TGuiApp.RequestStop();

                using var scope = _serviceProvider.CreateScope();
                var professorService =
                    scope.ServiceProvider.GetRequiredService<IProfessorService>();
                var professor = await professorService.GetProfessorByIdAsync(id);

                if (professor == null)
                {
                    MessageBox.ErrorQuery("Error", "Professor not found!", "OK");
                    return;
                }

                var confirm = MessageBox.Query(
                    "Confirm Delete",
                    $"Delete professor:\n{professor.AcademicTitle} {professor.FirstName} {professor.LastName} ({professor.UniversityIndex})?",
                    "Yes",
                    "No"
                );

                if (confirm == 0)
                {
                    await professorService.DeleteProfessorAsync(id);
                    MessageBox.Query("Success", "Professor deleted successfully!", "OK");
                }
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"Failed to delete professor:\n{ex.Message}", "OK");
            }
        };

        btnCancel.Clicked += () => TGuiApp.RequestStop();
        dialog.Add(label, idField, btnDelete, btnCancel);
        TGuiApp.Run(dialog);
    }

    private async Task RunQueryProfessorMostStudentsAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var queryService = scope.ServiceProvider.GetRequiredService<IQueryService>();
            var result = await queryService.GetProfessorWithMostStudentsAsync();

            TGuiApp.MainLoop.Invoke(() =>
            {
                if (result != null)
                {
                    var msg =
                        $"Professor: {result.FullName}\n"
                        + $"Index: {result.UniversityIndex}\n"
                        + $"Total Students: {result.TotalStudentCount}\n"
                        + $"Courses Taught: {result.CourseCount}";
                    MessageBox.Query("Query Result - Professor with Most Students", msg, "OK");
                }
                else
                {
                    MessageBox.Query("Query Result", "No professors found.", "OK");
                }
            });
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                MessageBox.ErrorQuery("Error", $"Query failed:\n{ex.Message}", "OK");
            });
        }
    }

    private async Task RunQueryCourseAveragesAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var departmentService = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
            var departments = await departmentService.GetAllDepartmentsAsync();

            var deptList = departments.Select(d => $"{d.Id}: {d.Name}").ToList();

            if (deptList.Count == 0)
            {
                TGuiApp.MainLoop.Invoke(() =>
                {
                    MessageBox.ErrorQuery("Error", "No departments found!", "OK");
                });
                return;
            }

            var dialog = new Dialog("Course Averages by Department", 70, 20);

            var label = new Label("Select Department:") { X = 2, Y = 1 };
            var listView = new ListView(deptList)
            {
                X = 2,
                Y = 2,
                Width = Dim.Fill(2),
                Height = 8,
            };
            var btnRun = new Button("Run Query") { X = Pos.Center(), Y = 11 };
            var btnClose = new Button("Close") { X = Pos.Center(), Y = 13 };

            btnRun.Clicked += async () =>
            {
                if (listView.SelectedItem >= 0 && listView.SelectedItem < departments.Count())
                {
                    var selectedDept = departments.ElementAt(listView.SelectedItem);
                    TGuiApp.RequestStop();

                    var queryService = scope.ServiceProvider.GetRequiredService<IQueryService>();
                    var results = await queryService.GetCourseAveragesForFacultyAsync(
                        selectedDept.Id
                    );

                    var resultText = string.Join(
                        "\n",
                        results.Select(r =>
                            $"{r.CourseCode} - {r.CourseName}: Avg={r.AverageGrade:F2}, Students={r.StudentCount}"
                        )
                    );

                    if (string.IsNullOrEmpty(resultText))
                        resultText = "No courses with grades found for this department.";

                    MessageBox.Query($"Course Averages - {selectedDept.Name}", resultText, "OK");
                }
            };

            btnClose.Clicked += () => TGuiApp.RequestStop();

            dialog.Add(label, listView, btnRun, btnClose);
            TGuiApp.Run(dialog);
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                MessageBox.ErrorQuery("Error", $"Query failed:\n{ex.Message}", "OK");
            });
        }
    }

    private async Task RunQueryStudentDifficultyAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var queryService = scope.ServiceProvider.GetRequiredService<IQueryService>();
            var result = await queryService.GetStudentWithHardestScheduleAsync();

            TGuiApp.MainLoop.Invoke(() =>
            {
                if (result != null)
                {
                    var msg =
                        $"Student: {result.FullName}\n"
                        + $"Index: {result.UniversityIndex}\n"
                        + $"Course ECTS: {result.CourseECTS}\n"
                        + $"Prerequisite ECTS: {result.PrerequisiteECTS}\n"
                        + $"Total Difficulty: {result.TotalDifficulty}";
                    MessageBox.Query("Query Result - Student with Hardest Schedule", msg, "OK");
                }
                else
                {
                    MessageBox.Query("Query Result", "No students found.", "OK");
                }
            });
        }
        catch (Exception ex)
        {
            TGuiApp.MainLoop.Invoke(() =>
            {
                MessageBox.ErrorQuery("Error", $"Query failed:\n{ex.Message}", "OK");
            });
        }
    }
}
