# Video Recording Feature Guide

## Overview
The Game of Life application now supports recording the simulation to video files using FFMediaToolkit and FFmpeg.

## Requirements

### FFmpeg DLLs
You need to place the following FFmpeg DLLs in the application directory (same folder as GameOfLife.exe):

- `avcodec-60.dll`
- `avformat-60.dll`
- `avutil-58.dll`
- `swresample-4.dll`
- `swscale-7.dll`

**Where to get FFmpeg DLLs:**
1. Download FFmpeg builds from: https://github.com/BtbN/FFmpeg-Builds/releases
2. Extract the archive and copy the DLL files from the `bin` folder
3. Place them in your application's output directory (e.g., `bin\Debug\net8.0-windows\`)

## How to Use

### Starting Recording
1. Click the **"🎥 Start Recording"** button in the Control Panel
2. Choose a location and filename for your video (default format is MP4)
3. Recording starts immediately - the status will show "● Recording" in red

### Stopping Recording
1. Click the **"⏹ Stop Recording"** button
2. The video will be finalized and saved
3. A message will show the number of frames captured and the output path

## Current Limitations

⚠️ **Important:** The current implementation provides the infrastructure for recording, but **frame capture is not yet integrated** with the game grid rendering.

### What's Implemented:
✅ Video recording infrastructure (VideoRecorder service)
✅ FFMediaToolkit integration
✅ UI controls for start/stop recording
✅ Commands in ViewModel

### What Needs Integration:
❌ Automatic frame capture during game updates
❌ Manual frame capture trigger

## Integration Steps (For Developer)

To fully enable recording, you need to integrate frame capture with the game grid rendering. Here's how:

### Option 1: Automatic Frame Capture (Recommended)

In `MainViewModel.cs`, modify the `Timer_Tick` method:

```csharp
private void Timer_Tick(object? sender, EventArgs e)
{
    _engine.NextGeneration();
    NotifyStatisticsChanged();

    // Capture frame if recording
    if (_videoRecorder?.IsRecording == true)
    {
        // You need access to the GameGrid visual element
        // This requires passing a reference from MainWindow
        CaptureCurrentFrame();
    }
}
```

### Option 2: Manual Capture in MainWindow.xaml.cs

Add a reference to the GameGrid control and capture frames:

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Subscribe to ViewModel updates
        if (DataContext is MainViewModel vm)
        {
            // Hook into the rendering or timer
            CompositionTarget.Rendering += (s, e) => CaptureFrameIfRecording(vm);
        }
    }

    private void CaptureFrameIfRecording(MainViewModel vm)
    {
        var recorder = vm.GetVideoRecorder();
        if (recorder?.IsRecording == true)
        {
            // Assuming you have a GameGrid control named "gameGrid"
            try
            {
                recorder.CaptureFrame(gameGrid, 1920, 1080);
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                System.Diagnostics.Debug.WriteLine($"Frame capture error: {ex.Message}");
            }
        }
    }
}
```

### Getting the GameGrid Visual

In your XAML, ensure the GameGrid has a name:

```xml
<controls:GameGrid x:Name="gameGrid" ... />
```

## Video Settings

Current default settings:
- **Resolution:** 1920x1080 (Full HD)
- **Frame Rate:** 30 FPS
- **Codec:** H.264
- **Quality:** CRF 17 (high quality)
- **Preset:** Fast

These can be modified in `MainViewModel.StartRecording()` method.

## Troubleshooting

### "Failed to start recording" Error
- Make sure FFmpeg DLLs are in the application directory
- Check that you have write permissions to the output location
- Verify the FFmpeg DLLs are for Windows x64 and compatible with your .NET runtime

### Recording Starts but No Video
- The frame capture integration is not yet complete
- Follow the integration steps above to enable actual frame recording

### Video File is Empty
- No frames were captured during the recording session
- Implement the frame capture integration

## Technical Details

- **Library:** FFMediaToolkit 4.8.1
- **Format:** MP4 container with H.264 video codec
- **Color Format:** BGRA32 (converted internally)
- **Memory:** Uses unsafe code for efficient pixel data copying

## Future Enhancements

Potential improvements:
- [ ] Configurable video resolution and frame rate
- [ ] Real-time preview of recording
- [ ] Recording duration/frame count display
- [ ] Pause/resume functionality
- [ ] Different codec options (VP9, AV1)
- [ ] Audio support (background music)

