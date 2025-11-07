using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FFMediaToolkit;
using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

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
            var renderBitmap = new RenderTargetBitmap(
                width,
                height,
                96, // DPI X
                96, // DPI Y
                PixelFormats.Pbgra32
            );
            renderBitmap.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using var memoryStream = new MemoryStream();
            encoder.Save(memoryStream);
            memoryStream.Position = 0;

            using var bitmap = new Bitmap(memoryStream);

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb
            );

            try
            {
                var videoWidth = _encoderSettings.VideoWidth;
                var videoHeight = _encoderSettings.VideoHeight;
                var stride = videoWidth * 4; // 4 bytes per pixel for BGRA32

                var frameData = new byte[stride * videoHeight];

                fixed (byte* dstBuffer = frameData)
                {
                    var srcPtr = (byte*)bitmapData.Scan0;
                    var srcStride = bitmapData.Stride;
                    var minHeight = Math.Min(bitmap.Height, videoHeight);
                    var minWidth = Math.Min(bitmap.Width, videoWidth);
                    const int bytesPerPixel = 4;
                    var copyWidth = minWidth * bytesPerPixel;

                    for (var y = 0; y < minHeight; y++)
                        Buffer.MemoryCopy(
                            srcPtr + y * srcStride,
                            dstBuffer + y * stride,
                            copyWidth,
                            copyWidth
                        );
                }

                var imageData = new ImageData(
                    frameData,
                    ImagePixelFormat.Bgra32,
                    videoWidth,
                    videoHeight
                );

                _mediaOutput.Video.AddFrame(imageData);
                FrameCount++;
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
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
