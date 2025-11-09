using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GameOfLife.Models;
using GameOfLife.Models.Coloring;
using GameOfLife.Services;
using Microsoft.Win32;

namespace GameOfLife.ViewModels;

/// <summary>
///     Main ViewModel for the Game of Life application
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly DispatcherTimer _timer;
    private int _animationSpeed;
    private string _cellColor;
    private string _cellShape;
    private IColoringModel? _currentColoringModel;
    private GameOfLifeEngine _engine;
    private int _gridHeight;
    private int _gridWidth;
    private bool _isEditing;
    private bool _isRecording;
    private bool _isRunning;
    private bool _patternMergeMode = true;
    private int _patternX;
    private int _patternY;
    private int _refreshTrigger;
    private string _rulesText;
    private string _selectedColoringModel = "Standard";
    private string _selectedPattern = "";
    private VideoRecorder? _videoRecorder;
    private double _zoomLevel;

    public MainViewModel()
    {
        _gridWidth = 100;
        _gridHeight = 100;
        _engine = new GameOfLifeEngine(_gridWidth, _gridHeight);
        _rulesText = "B3/S23";
        _animationSpeed = 100;
        _zoomLevel = 1.0;
        _cellColor = "#FF00FF00";
        _cellShape = "Rectangle";
        _isEditing = true;

        _timer = new DispatcherTimer();
        _timer.Tick += Timer_Tick;

        InitializePatterns();
        InitializeColorings();
        InitializeDefaultColoring();
        InitializeCommands();
        UpdateTimerInterval();
    }

    private void InitializeCommands()
    {
        StartCommand = new RelayCommand(Start, () => !IsRunning);
        StopCommand = new RelayCommand(Stop, () => IsRunning);
        StepCommand = new RelayCommand(Step, () => !IsRunning);
        ClearCommand = new RelayCommand(Clear, () => !IsRunning);
        RandomizeCommand = new RelayCommand(Randomize, () => !IsRunning);
        SaveCommand = new RelayCommand(Save);
        LoadCommand = new RelayCommand(Load, () => !IsRunning);
        ApplyRulesCommand = new RelayCommand(ApplyRules, () => !IsRunning);
        PlacePatternCommand = new RelayCommand(
            PlacePattern,
            () => !IsRunning && !string.IsNullOrEmpty(SelectedPattern)
        );
        ClearPatternCommand = new RelayCommand(ClearPattern, () => !IsRunning);
        ChangeColoringCommand = new RelayCommand(ChangeColoring, () => !IsRunning);
        ResizeGridCommand = new RelayCommand(ResizeGrid, () => !IsRunning);
        ZoomInCommand = new RelayCommand(ZoomIn);
        ZoomOutCommand = new RelayCommand(ZoomOut);
        StartRecordingCommand = new RelayCommand(StartRecording, () => !IsRecording);
        StopRecordingCommand = new RelayCommand(StopRecording, () => IsRecording);
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _engine.NextGeneration(CurrentColoringModel);

        // Update coloring model state (e.g., age increments)
        if (CurrentColoringModel != null)
            CurrentColoringModel.NextGeneration();

        NotifyStatisticsChanged();
    }

    private void UpdateTimerInterval()
    {
        // AnimationSpeed is from 1-200, convert to milliseconds (inverse relationship)
        var interval = Math.Max(10, 210 - AnimationSpeed);
        _timer.Interval = TimeSpan.FromMilliseconds(interval);
    }

    private void NotifyStatisticsChanged()
    {
        OnPropertyChanged(nameof(Generation));
        OnPropertyChanged(nameof(BornCells));
        OnPropertyChanged(nameof(DeadCells));
        OnPropertyChanged(nameof(LivingCells));
        OnPropertyChanged(nameof(Engine));
        // Force trigger property changed to update the view
        Application.Current?.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
    }

    public void ToggleCell(int x, int y)
    {
        if (!IsRunning)
        {
            var wasAlive = _engine.GetCell(x, y);
            _engine.ToggleCell(x, y);

            OnPropertyChanged(nameof(Engine));
            OnPropertyChanged(nameof(LivingCells));
        }
    }

    private void PlacePattern()
    {
        if (string.IsNullOrEmpty(SelectedPattern))
            return;

        var patterns = PresetPatterns.GetAllPatterns();
        if (!patterns.ContainsKey(SelectedPattern))
            return;

        try
        {
            var pattern = patterns[SelectedPattern];
            _engine.PlacePattern(pattern, PatternX, PatternY, PatternMergeMode);
            NotifyStatisticsChanged();
            RefreshTrigger++;
        }
        catch
        {
            // Silently handle errors
        }
    }

    private void ClearPattern()
    {
        _engine.Clear();
        NotifyStatisticsChanged();
        RefreshTrigger++;
    }

    private void ChangeColoring()
    {
        try
        {
            // Create the appropriate coloring model based on selection
            CurrentColoringModel = ColoringModelFactory.CreateColoring(
                SelectedColoringModel,
                _engine.Width,
                _engine.Height
            );

            // Initialize colors for the current grid state
            CurrentColoringModel.InitializeColorsForGrid(_engine.GetStateCopy());

            // Force re-render to apply new coloring
            RefreshTrigger++;
        }
        catch
        {
            // Silently handle errors
        }
    }

    public void InitializePatterns()
    {
        var patterns = PresetPatterns.GetAllPatterns();
        AvailablePatterns.Clear();
        foreach (var patternName in patterns.Keys)
            AvailablePatterns.Add(patternName);
        if (AvailablePatterns.Count > 0)
            SelectedPattern = AvailablePatterns[0];
    }

    public void InitializeColorings()
    {
        var colorings = ColoringModelFactory.GetAvailableColorings();
        AvailableColorings.Clear();
        foreach (var coloring in colorings)
            AvailableColorings.Add(coloring);
        if (AvailableColorings.Count > 0)
            SelectedColoringModel = AvailableColorings[0];
    }

    public void InitializeDefaultColoring()
    {
        CurrentColoringModel = ColoringModelFactory.CreateColoring(
            "Standard",
            _engine.Width,
            _engine.Height
        );
    }

    #region Properties

    public GameOfLifeEngine Engine
    {
        get => _engine;
        private set => SetProperty(ref _engine, value);
    }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (SetProperty(ref _isRunning, value))
            {
                IsEditing = !value;
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        private set => SetProperty(ref _isEditing, value);
    }

    public int GridWidth
    {
        get => _gridWidth;
        set => SetProperty(ref _gridWidth, value);
    }

    public int GridHeight
    {
        get => _gridHeight;
        set => SetProperty(ref _gridHeight, value);
    }

    public string RulesText
    {
        get => _rulesText;
        set => SetProperty(ref _rulesText, value);
    }

    public int AnimationSpeed
    {
        get => _animationSpeed;
        set
        {
            if (SetProperty(ref _animationSpeed, value))
                UpdateTimerInterval();
        }
    }

    public double ZoomLevel
    {
        get => _zoomLevel;
        set => SetProperty(ref _zoomLevel, Math.Max(0.5, Math.Min(5.0, value)));
    }

    public string CellColor
    {
        get => _cellColor;
        set => SetProperty(ref _cellColor, value);
    }

    public string CellShape
    {
        get => _cellShape;
        set => SetProperty(ref _cellShape, value);
    }

    public int RefreshTrigger
    {
        get => _refreshTrigger;
        set => SetProperty(ref _refreshTrigger, value);
    }

    public bool IsRecording
    {
        get => _isRecording;
        set
        {
            if (SetProperty(ref _isRecording, value))
                OnPropertyChanged(nameof(RecordingStatusText));
        }
    }

    public ObservableCollection<string> AvailablePatterns { get; } = new();

    public ObservableCollection<string> AvailableColorings { get; } = new();

    public string SelectedPattern
    {
        get => _selectedPattern;
        set => SetProperty(ref _selectedPattern, value);
    }

    public string SelectedColoringModel
    {
        get => _selectedColoringModel;
        set => SetProperty(ref _selectedColoringModel, value);
    }

    public int PatternX
    {
        get => _patternX;
        set => SetProperty(ref _patternX, value);
    }

    public int PatternY
    {
        get => _patternY;
        set => SetProperty(ref _patternY, value);
    }

    public bool PatternMergeMode
    {
        get => _patternMergeMode;
        set => SetProperty(ref _patternMergeMode, value);
    }

    public IColoringModel? CurrentColoringModel
    {
        get => _currentColoringModel;
        private set => SetProperty(ref _currentColoringModel, value);
    }

    public ObservableCollection<string> AvailableShapes { get; } =
        new() { "Rectangle", "Ellipse", "RoundedRectangle" };

    public string StatusText => IsRunning ? "Running" : "Editing";

    public string RecordingStatusText => IsRecording ? "● Recording" : "Not Recording";

    public int VideoWidth { get; private set; }

    public int VideoHeight { get; private set; }

    public long Generation => _engine.Generation;
    public long BornCells => _engine.BornCells;
    public long DeadCells => _engine.DeadCells;
    public int LivingCells => _engine.GetLivingCellsCount();

    #endregion

    #region Commands

    public ICommand StartCommand { get; private set; } = null!;
    public ICommand StopCommand { get; private set; } = null!;
    public ICommand StepCommand { get; private set; } = null!;
    public ICommand ClearCommand { get; private set; } = null!;
    public ICommand RandomizeCommand { get; private set; } = null!;
    public ICommand SaveCommand { get; private set; } = null!;
    public ICommand LoadCommand { get; private set; } = null!;
    public ICommand ApplyRulesCommand { get; private set; } = null!;
    public ICommand ResizeGridCommand { get; private set; } = null!;
    public ICommand ZoomInCommand { get; private set; } = null!;
    public ICommand ZoomOutCommand { get; private set; } = null!;
    public ICommand StartRecordingCommand { get; private set; } = null!;
    public ICommand StopRecordingCommand { get; private set; } = null!;
    public ICommand PlacePatternCommand { get; private set; } = null!;
    public ICommand ClearPatternCommand { get; private set; } = null!;
    public ICommand ChangeColoringCommand { get; private set; } = null!;

    #endregion

    #region Command Implementations

    private void Start()
    {
        ApplyRules();
        IsRunning = true;
        _timer.Start();
    }

    private void Stop()
    {
        _timer.Stop();
        IsRunning = false;
    }

    private void Step()
    {
        ApplyRules();
        _engine.NextGeneration(CurrentColoringModel);

        // Update coloring model state (e.g., age increments)
        if (CurrentColoringModel != null)
            CurrentColoringModel.NextGeneration();

        NotifyStatisticsChanged();
    }

    private void Clear()
    {
        _engine.Clear();

        // Clear coloring model state as well
        if (CurrentColoringModel != null)
            CurrentColoringModel.Clear();

        NotifyStatisticsChanged();
        // Force immediate view refresh
        RefreshTrigger++;
    }

    private void Randomize()
    {
        _engine.Randomize();

        // Initialize colors for the randomized grid if using a coloring model that tracks state
        if (CurrentColoringModel != null)
            CurrentColoringModel.InitializeColorsForGrid(_engine.GetStateCopy());

        NotifyStatisticsChanged();
        // Force immediate view refresh
        RefreshTrigger++;
    }

    private void Save()
    {
        var dialog = new SaveFileDialog
        {
            Filter =
                "Game of Life files (*.gol)|*.gol|Text files (*.txt)|*.txt|All files (*.*)|*.*",
            DefaultExt = "gol",
            FileName = $"gameoflife_{DateTime.Now:yyyyMMdd_HHmmss}.gol",
        };

        if (dialog.ShowDialog() == true)
            try
            {
                var state = new GameState(
                    _engine.Width,
                    _engine.Height,
                    _engine.GetStateCopy(),
                    _engine.Rules,
                    _engine.Generation
                );

                // Save coloring model information
                state.ColoringModelName = CurrentColoringModel?.Name ?? "Standard";
                state.ColoringData = CurrentColoringModel?.Serialize() ?? new List<string>();

                state.SaveToFile(dialog.FileName);
                MessageBox.Show(
                    "Game state saved successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving file: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
    }

    private void Load()
    {
        var dialog = new OpenFileDialog
        {
            Filter =
                "Game of Life files (*.gol)|*.gol|Text files (*.txt)|*.txt|All files (*.*)|*.*",
        };

        if (dialog.ShowDialog() == true)
            try
            {
                var state = GameState.LoadFromFile(dialog.FileName);

                if (state.Width != _engine.Width || state.Height != _engine.Height)
                {
                    _gridWidth = state.Width;
                    _gridHeight = state.Height;
                    _engine = new GameOfLifeEngine(state.Width, state.Height, state.Rules);
                    OnPropertyChanged(nameof(GridWidth));
                    OnPropertyChanged(nameof(GridHeight));
                    OnPropertyChanged(nameof(Engine));
                }
                // Restore coloring model
                SelectedColoringModel = state.ColoringModelName;
                CurrentColoringModel = ColoringModelFactory.CreateColoring(
                    state.ColoringModelName,
                    _engine.Width,
                    _engine.Height
                );

                // Restore coloring model state
                if (state.ColoringData.Count > 0)
                {
                    CurrentColoringModel.Deserialize(state.ColoringData);
                }
                else
                {
                    // If no saved coloring data, initialize with current state
                    CurrentColoringModel.InitializeColorsForGrid(_engine.GetStateCopy());
                }


                _engine.SetState(state.Cells);
                _engine.Rules = state.Rules;
                RulesText = state.Rules.ToString();

                // Restore coloring model
                SelectedColoringModel = state.ColoringModelName;
                CurrentColoringModel = ColoringModelFactory.CreateColoring(
                    state.ColoringModelName,
                    _engine.Width,
                    _engine.Height
                );

                // Restore coloring model state
                if (state.ColoringData.Count > 0)
                {
                    CurrentColoringModel.Deserialize(state.ColoringData);
                }
                else
                {
                    // If no saved coloring data, initialize with current state
                    CurrentColoringModel.InitializeColorsForGrid(_engine.GetStateCopy());
                }

                NotifyStatisticsChanged();
                // Force immediate screen refresh
                RefreshTrigger++;

                MessageBox.Show(
                    "Game state loaded successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading file: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
    }

    private void ApplyRules()
    {
        if (GameRules.TryParse(RulesText, out var rules) && rules != null)
        {
            _engine.Rules = rules;
        }
        else
        {
            MessageBox.Show(
                "Invalid rules format. Using default: B3/S23",
                "Warning",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            RulesText = "B3/S23";
            _engine.Rules = GameRules.ConwayDefault();
        }
    }

    private void ResizeGrid()
    {
        if (GridWidth < 10 || GridWidth > 1000 || GridHeight < 10 || GridHeight > 1000)
        {
            MessageBox.Show(
                "Grid size must be between 10 and 1000",
                "Invalid Size",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        _engine = new GameOfLifeEngine(GridWidth, GridHeight, _engine.Rules);
        OnPropertyChanged(nameof(Engine));
        NotifyStatisticsChanged();
    }

    private void ZoomIn()
    {
        ZoomLevel = Math.Min(5.0, ZoomLevel + 0.25);
    }

    private void ZoomOut()
    {
        ZoomLevel = Math.Max(0.5, ZoomLevel - 0.25);
    }

    private void StartRecording()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter = "MP4 Video (*.mp4)|*.mp4|AVI Video (*.avi)|*.avi|All files (*.*)|*.*",
                DefaultExt = "mp4",
                FileName = $"gameoflife_{DateTime.Now:yyyyMMdd_HHmmss}.mp4",
            };

            if (dialog.ShowDialog() == true)
            {
                // Initialize video recorder
                _videoRecorder = new VideoRecorder();

                // Fixed 1080p output - FFmpeg will scale the input
                VideoWidth = 1920;
                VideoHeight = 1080;


                _videoRecorder.StartRecording(dialog.FileName, VideoWidth, VideoHeight, 15);
                IsRecording = true;
            }
        }
        catch
        {
            // Silently handle errors
            IsRecording = false;
            _videoRecorder?.Dispose();
            _videoRecorder = null;
        }
    }

    private void StopRecording()
    {
        try
        {
            if (_videoRecorder != null)
            {
                _videoRecorder.StopRecording();
                _videoRecorder.Dispose();
                _videoRecorder = null;
                IsRecording = false;
            }
        }
        catch
        {
            // Silently handle errors
        }
        finally
        {
            IsRecording = false;
            _videoRecorder?.Dispose();
            _videoRecorder = null;
        }
    }

    public VideoRecorder? GetVideoRecorder()
    {
        return _videoRecorder;
    }

    #endregion
}
