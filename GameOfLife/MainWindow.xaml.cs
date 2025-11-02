using System.Windows;
using System.Windows.Media;
using GameOfLife.ViewModels;

namespace GameOfLife;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel? ViewModel => DataContext as MainViewModel;

    public MainWindow()
    {
        InitializeComponent();

        // Hook into rendering to capture frames when recording
        CompositionTarget.Rendering += OnRendering;
    }

    private void OnRendering(object? sender, System.EventArgs e)
    {
        // Capture frame if recording is active
        if (ViewModel?.GetVideoRecorder()?.IsRecording == true)
        {
            try
            {
                // Capture the current frame from the GameGrid control
                // Using 1920x1080 resolution (can be adjusted)
                ViewModel.GetVideoRecorder()?.CaptureFrame(GameGridControl, 1920, 1080);
            }
            catch (System.Exception ex)
            {
                // Log error but don't stop the application
                System.Diagnostics.Debug.WriteLine($"Frame capture error: {ex.Message}");
            }
        }
    }

    protected override void OnClosed(System.EventArgs e)
    {
        // Clean up rendering event
        CompositionTarget.Rendering -= OnRendering;

        // Stop recording if still active
        ViewModel?.GetVideoRecorder()?.StopRecording();

        base.OnClosed(e);
    }
}
