using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FFMediaToolkit;
using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;

namespace GameOfLife.Services;

/// <summary>
/// Service for recording the game grid to video using FFMediaToolkit
/// </summary>
public class VideoRecorder : IDisposable
{
    private MediaOutput? _mediaOutput;
    private VideoEncoderSettings? _encoderSettings;
    private bool _isRecording;
    private string? _outputPath;
    private int _frameCount;
    private DateTime _recordingStartTime;

    public bool IsRecording => _isRecording;
    public string? OutputPath => _outputPath;
    public int FrameCount => _frameCount;
    public TimeSpan RecordingDuration => _isRecording ? DateTime.Now - _recordingStartTime : TimeSpan.Zero;

    static VideoRecorder()
    {
        // Set FFmpeg path - assumes FFmpeg DLLs are in the application directory
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        FFmpegLoader.FFmpegPath = appDirectory;
    }

    /// <summary>
    /// Start recording to a video file
    /// </summary>
    /// <param name="outputPath">Path to the output video file</param>
    /// <param name="width">Video width in pixels</param>
    /// <param name="height">Video height in pixels</param>
    /// <param name="framerate">Frames per second (default: 30)</param>
    public void StartRecording(string outputPath, int width, int height, int framerate = 30)
    {
        if (_isRecording)
        {
            throw new InvalidOperationException("Recording is already in progress");
        }

        try
        {
            _outputPath = outputPath;
            _frameCount = 0;
            _recordingStartTime = DateTime.Now;

            // Configure video encoder settings
            _encoderSettings = new VideoEncoderSettings(
                width: width,
                height: height,
                framerate: framerate,
                codec: VideoCodec.H264
            );

            _encoderSettings.EncoderPreset = EncoderPreset.Fast;
            _encoderSettings.CRF = 17; // Quality (lower = better quality, range: 0-51)

            // Create media output
            _mediaOutput = MediaBuilder.CreateContainer(outputPath).WithVideo(_encoderSettings).Create();

            _isRecording = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to start recording: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Capture a frame from a Visual element
    /// </summary>
    public unsafe void CaptureFrame(Visual visual, int width, int height)
    {
        if (!_isRecording || _mediaOutput == null || _encoderSettings == null)
        {
            throw new InvalidOperationException("Recording is not in progress");
        }

        try
        {
            // Render the visual to a bitmap
            var renderBitmap = new RenderTargetBitmap(
                width,
                height,
                96, // DPI X
                96, // DPI Y
                PixelFormats.Pbgra32
            );
            renderBitmap.Render(visual);

            // Convert to System.Drawing.Bitmap
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using var memoryStream = new MemoryStream();
            encoder.Save(memoryStream);
            memoryStream.Position = 0;

            using var bitmap = new System.Drawing.Bitmap(memoryStream);

            // Create ImageData manually from bitmap
            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                // Calculate the buffer size needed
                int videoWidth = _encoderSettings.VideoWidth;
                int videoHeight = _encoderSettings.VideoHeight;
                int stride = videoWidth * 4; // 4 bytes per pixel for BGRA32

                // Create a byte array for the frame data
                byte[] frameData = new byte[stride * videoHeight];

                // Copy pixel data from bitmap to frame buffer
                fixed (byte* dstBuffer = frameData)
                {
                    byte* srcPtr = (byte*)bitmapData.Scan0;
                    int srcStride = bitmapData.Stride;
                    int minHeight = Math.Min(bitmap.Height, videoHeight);
                    int minWidth = Math.Min(bitmap.Width, videoWidth);
                    int bytesPerPixel = 4;
                    int copyWidth = minWidth * bytesPerPixel;

                    for (int y = 0; y < minHeight; y++)
                    {
                        Buffer.MemoryCopy(
                            srcPtr + y * srcStride,
                            dstBuffer + y * stride,
                            copyWidth,
                            copyWidth
                        );
                    }
                }

                // Create ImageData from the byte array
                var imageData = new ImageData(frameData, ImagePixelFormat.Bgra32, videoWidth, videoHeight);

                // Write frame to video
                _mediaOutput.Video.AddFrame(imageData);
                _frameCount++;
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

    /// <summary>
    /// Stop recording and finalize the video file
    /// </summary>
    public void StopRecording()
    {
        if (!_isRecording)
        {
            return;
        }

        try
        {
            _mediaOutput?.Dispose();
            _mediaOutput = null;
            _isRecording = false;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to stop recording: {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        StopRecording();
    }
}

