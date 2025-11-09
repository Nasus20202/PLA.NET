using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FFMediaToolkit;
using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;

namespace GameOfLife.Services;

public class VideoRecorder : IDisposable
{
    private VideoEncoderSettings? _encoderSettings;
    private MediaOutput? _mediaOutput;
    private DateTime _recordingStartTime;

    static VideoRecorder()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        FFmpegLoader.FFmpegPath = appDirectory;
    }

    public bool IsRecording { get; private set; }

    public string? OutputPath { get; private set; }

    public int FrameCount { get; private set; }

    public TimeSpan RecordingDuration =>
        IsRecording ? DateTime.Now - _recordingStartTime : TimeSpan.Zero;

    public void Dispose()
    {
        StopRecording();
    }

    public void StartRecording(string outputPath, int width, int height, int framerate = 30)
    {
        if (IsRecording)
            throw new InvalidOperationException("Recording is already in progress");

        try
        {
            OutputPath = outputPath;
            FrameCount = 0;
            _recordingStartTime = DateTime.Now;

            _encoderSettings = new VideoEncoderSettings(width, height, framerate, VideoCodec.H264)
            {
                EncoderPreset = EncoderPreset.Fast,
                CRF = 17,
            };

            _mediaOutput = MediaBuilder
                .CreateContainer(outputPath)
                .WithVideo(_encoderSettings)
                .Create();

            IsRecording = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to start recording: {ex.Message}", ex);
        }
    }

    public unsafe void CaptureFrame(Visual visual, int width, int height)
    {
        if (!IsRecording || _mediaOutput == null || _encoderSettings == null)
            throw new InvalidOperationException("Recording is not in progress");

        try
        {
            // Get actual size of the visual (for ScrollViewer, this is the viewport size)
            int renderWidth = width;
            int renderHeight = height;

            if (visual is FrameworkElement element)
            {
                renderWidth = (int)Math.Ceiling(element.ActualWidth);
                renderHeight = (int)Math.Ceiling(element.ActualHeight);

                // Ensure even dimensions
                if (renderWidth % 2 != 0) renderWidth++;
                if (renderHeight % 2 != 0) renderHeight++;
            }

            // Render the visible area
            var renderBitmap = new RenderTargetBitmap(
                renderWidth,
                renderHeight,
                96,
                96,
                PixelFormats.Pbgra32
            );
            renderBitmap.Render(visual);

            // Scale and center in video frame
            var videoWidth = _encoderSettings.VideoWidth;
            var videoHeight = _encoderSettings.VideoHeight;

            var scaleX = (double)videoWidth / renderWidth;
            var scaleY = (double)videoHeight / renderHeight;
            var scaleFactor = Math.Min(scaleX, scaleY);

            var scaledWidth = (int)(renderWidth * scaleFactor);
            var scaledHeight = (int)(renderHeight * scaleFactor);

            // Ensure even dimensions
            if (scaledWidth % 2 != 0) scaledWidth--;
            if (scaledHeight % 2 != 0) scaledHeight--;

            var transform = new ScaleTransform(
                (double)scaledWidth / renderWidth,
                (double)scaledHeight / renderHeight
            );

            var scaledBitmap = new TransformedBitmap(renderBitmap, transform);

            // Create final frame with black background, centered content
            var finalBitmap = new RenderTargetBitmap(
                videoWidth,
                videoHeight,
                96,
                96,
                PixelFormats.Pbgra32
            );

            var drawingVisual = new DrawingVisual();
            using (var dc = drawingVisual.RenderOpen())
            {
                dc.DrawRectangle(System.Windows.Media.Brushes.Black, null, new Rect(0, 0, videoWidth, videoHeight));
                var x = (videoWidth - scaledWidth) / 2.0;
                var y = (videoHeight - scaledHeight) / 2.0;
                dc.DrawImage(scaledBitmap, new Rect(x, y, scaledWidth, scaledHeight));
            }

            finalBitmap.Render(drawingVisual);

            // Extract pixel data directly from RenderTargetBitmap
            var stride = videoWidth * 4;
            var frameData = new byte[stride * videoHeight];
            finalBitmap.CopyPixels(frameData, stride, 0);

            var imageData = new ImageData(
                frameData,
                ImagePixelFormat.Bgra32,
                videoWidth,
                videoHeight
            );

            _mediaOutput.Video.AddFrame(imageData);
            FrameCount++;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to capture frame: {ex.Message}", ex);
        }
    }

    public void StopRecording()
    {
        if (!IsRecording)
            return;

        try
        {
            _mediaOutput?.Dispose();
            _mediaOutput = null;
            IsRecording = false;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to stop recording: {ex.Message}", ex);
        }
    }
}
