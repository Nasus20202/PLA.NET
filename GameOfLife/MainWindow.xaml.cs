using System.Windows.Media;
using GameOfLife.ViewModels;

namespace GameOfLife;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private MainViewModel? ViewModel => DataContext as MainViewModel;

    public MainWindow()
    {
        InitializeComponent();

        // Hook into rendering to capture frames when recording
        CompositionTarget.Rendering += OnRendering;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        // Capture frame if recording is active
        if (ViewModel?.GetVideoRecorder()?.IsRecording == true)
        {
            try
            {
                // Capture the current frame from the GameGrid control using actual dimensions
                int width = ViewModel.VideoWidth;
                int height = ViewModel.VideoHeight;

                ViewModel.GetVideoRecorder()?.CaptureFrame(GameGridControl, width, height);
            }
            catch (Exception ex)
            {
                // Log error but don't stop the application
                System.Diagnostics.Debug.WriteLine($"Frame capture error: {ex.Message}");
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Clean up rendering event
        CompositionTarget.Rendering -= OnRendering;

        // Stop recording if still active
        ViewModel?.GetVideoRecorder()?.StopRecording();

        base.OnClosed(e);
    }
}
