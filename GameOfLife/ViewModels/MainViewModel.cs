using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GameOfLife.Models;
using Microsoft.Win32;

namespace GameOfLife.ViewModels;

/// <summary>
/// Main ViewModel for the Game of Life application
/// </summary>
public class MainViewModel : ViewModelBase
{
    private GameOfLifeEngine _engine;
    private DispatcherTimer _timer;
    private bool _isRunning;
    private bool _isEditing;
    private int _gridWidth;
    private int _gridHeight;
    private string _rulesText;
    private int _animationSpeed;
    private double _zoomLevel;
    private string _cellColor;
    private string _cellShape;
    private int _refreshTrigger;

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
        ResizeGridCommand = new RelayCommand(ResizeGrid, () => !IsRunning);
        ZoomInCommand = new RelayCommand(ZoomIn);
        ZoomOutCommand = new RelayCommand(ZoomOut);
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
            {
                UpdateTimerInterval();
            }
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

    public ObservableCollection<string> AvailableShapes { get; } =
        new() { "Rectangle", "Ellipse", "RoundedRectangle" };

    public string StatusText => IsRunning ? "Running" : "Editing";

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
        _engine.NextGeneration();
        NotifyStatisticsChanged();
    }

    private void Clear()
    {
        _engine.Clear();
        NotifyStatisticsChanged();
        // Wymuś natychmiastowe odświeżenie widoku
        RefreshTrigger++;
    }

    private void Randomize()
    {
        _engine.Randomize(0.3);
        NotifyStatisticsChanged();
        // Wymuś natychmiastowe odświeżenie widoku
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
        {
            try
            {
                var state = new GameState(
                    _engine.Width,
                    _engine.Height,
                    _engine.GetStateCopy(),
                    _engine.Rules,
                    _engine.Generation
                );
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
    }

    private void Load()
    {
        var dialog = new OpenFileDialog
        {
            Filter =
                "Game of Life files (*.gol)|*.gol|Text files (*.txt)|*.txt|All files (*.*)|*.*",
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var state = GameState.LoadFromFile(dialog.FileName);

                // Resize grid if needed
                if (state.Width != _engine.Width || state.Height != _engine.Height)
                {
                    _gridWidth = state.Width;
                    _gridHeight = state.Height;
                    _engine = new GameOfLifeEngine(state.Width, state.Height, state.Rules);
                    OnPropertyChanged(nameof(GridWidth));
                    OnPropertyChanged(nameof(GridHeight));
                    OnPropertyChanged(nameof(Engine));
                }

                _engine.SetState(state.Cells);
                _engine.Rules = state.Rules;
                RulesText = state.Rules.ToString();

                NotifyStatisticsChanged();
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
                $"Invalid rules format. Using default: B3/S23",
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

    #endregion

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _engine.NextGeneration();
        NotifyStatisticsChanged();
    }

    private void UpdateTimerInterval()
    {
        // AnimationSpeed is from 1-200, convert to milliseconds (inverse relationship)
        int interval = Math.Max(10, 210 - AnimationSpeed);
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
        System.Windows.Application.Current?.Dispatcher.Invoke(
            () => { },
            System.Windows.Threading.DispatcherPriority.Render
        );
    }

    public void ToggleCell(int x, int y)
    {
        if (!IsRunning)
        {
            _engine.ToggleCell(x, y);
            OnPropertyChanged(nameof(Engine));
            OnPropertyChanged(nameof(LivingCells));
        }
    }
}
