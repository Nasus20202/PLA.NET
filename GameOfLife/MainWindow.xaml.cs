using System.Diagnostics;
using System.Windows.Media;
using GameOfLife.ViewModels;

namespace GameOfLife;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        // Hook into rendering to capture frames when recording
        CompositionTarget.Rendering += OnRendering;
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    private void OnRendering(object? sender, EventArgs e)
    {
        if (ViewModel?.GetVideoRecorder()?.IsRecording != true)
            return;
        try
        {
            var width = ViewModel.VideoWidth;
            var height = ViewModel.VideoHeight;

            // Render the ScrollViewer to capture only the visible area
            ViewModel.GetVideoRecorder()?.CaptureFrame(GameScrollViewer, width, height);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Frame capture error: {ex.Message}");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        CompositionTarget.Rendering -= OnRendering;

        ViewModel?.GetVideoRecorder()?.StopRecording();

        base.OnClosed(e);
    }
}
