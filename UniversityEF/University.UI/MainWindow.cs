using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;
using University.UI.Dialogs;
using University.UI.Views;
using TGuiApp = Terminal.Gui.Application;

namespace University.UI;

public class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly FrameView _contentFrame;
    private BaseView? _currentView;

    public MainWindow(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Title = "University Management System - EF Core (Press F10 to quit)";

        var menu = CreateMenuBar();

        _contentFrame = new FrameView("Content")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
        };

        var statusBar = new StatusBar(
            new StatusItem[]
            {
                new StatusItem(Key.F10, "~F10~ Quit", () => TGuiApp.RequestStop()),
                new StatusItem(Key.F5, "~F5~ Refresh", RefreshCurrentView),
            }
        );

        Add(menu, _contentFrame, statusBar);
        ShowWelcomeScreen();
    }

    private MenuBar CreateMenuBar()
    {
        return new MenuBar(
            new MenuBarItem[]
            {
                new MenuBarItem(
                    "_File",
                    new MenuItem[]
                    {
                        new MenuItem("_Generate Data", "", ShowGenerateDataDialog),
                        null!, // Separator
                        new MenuItem("_Quit", "", () => TGuiApp.RequestStop()),
                    }
                ),
                new MenuBarItem(
                    "_Browse",
                    new MenuItem[]
                    {
                        new MenuItem("_Students", "", () => ShowView<StudentsView>()),
                        new MenuItem("_Professors", "", () => ShowView<ProfessorsView>()),
                        new MenuItem("_Courses", "", () => ShowView<CoursesView>()),
                        new MenuItem("_Departments", "", () => ShowView<DepartmentsView>()),
                        new MenuItem("_Offices", "", () => ShowView<OfficesView>()),
                        new MenuItem("C_ounters", "", () => ShowView<CountersView>()),
                        new MenuItem("_Enrollments", "", () => ShowView<EnrollmentsView>()),
                    }
                ),
                new MenuBarItem(
                    "_Queries",
                    new MenuItem[]
                    {
                        new MenuItem(
                            "Professor with _Most Students",
                            "",
                            ShowProfessorMostStudentsDialog
                        ),
                        new MenuItem("Course _Averages", "", ShowCourseAveragesDialog),
                        new MenuItem("Student _Difficulty", "", ShowStudentDifficultyDialog),
                    }
                ),
            }
        );
    }

    private void ShowWelcomeScreen()
    {
        _contentFrame.RemoveAll();
        _currentView = null;

        var welcome = new Label()
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Text = "Welcome to University Management System",
            TextAlignment = TextAlignment.Centered,
        };
        _contentFrame.Add(welcome);
    }

    private void ShowView<T>()
        where T : BaseView
    {
        _contentFrame.RemoveAll();

        var view = Activator.CreateInstance(typeof(T), _serviceProvider) as T;
        if (view != null)
        {
            _currentView = view;
            _contentFrame.Add(view);
            Task.Run(async () => await view.LoadDataAsync()).Wait();
        }
    }

    private void RefreshCurrentView()
    {
        if (_currentView != null)
        {
            Task.Run(async () => await _currentView.LoadDataAsync()).Wait();
        }
    }

    private void ShowGenerateDataDialog()
    {
        var dialog = new GenerateDataDialog(_serviceProvider);
        TGuiApp.Run(dialog);
    }

    private void ShowCourseAveragesDialog()
    {
        var dialog = new CourseAveragesDialog(_serviceProvider);
        Task.Run(async () => await dialog.LoadDepartmentsAsync()).Wait();
        TGuiApp.Run(dialog);
    }

    private void ShowProfessorMostStudentsDialog()
    {
        var dialog = new ProfessorMostStudentsDialog(_serviceProvider);
        TGuiApp.Run(dialog);
    }

    private void ShowStudentDifficultyDialog()
    {
        var dialog = new StudentDifficultyDialog(_serviceProvider);
        TGuiApp.Run(dialog);
    }
}
