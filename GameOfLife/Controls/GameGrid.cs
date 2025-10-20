using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GameOfLife.Models;
using GameOfLife.ViewModels;

namespace GameOfLife.Controls;

/// <summary>
/// Custom control for rendering and interacting with the Game of Life grid
/// Optimized version using DrawingVisual for low memory usage
/// </summary>
public class GameGrid : FrameworkElement
{
    private GameOfLifeEngine? _engine;
    private MainViewModel? _viewModel;
    private double _cellSize = 8;
    private bool[,]? _previousState;
    private long _lastGeneration = -1;
    private DrawingVisual? _backgroundVisual;
    private DrawingVisual? _cellsVisual;
    private VisualCollection? _visualChildren;
    private bool _forceRender = false;
    private int _renderSkipCounter = 0;

    public static readonly DependencyProperty EngineProperty =
        DependencyProperty.Register(nameof(Engine), typeof(GameOfLifeEngine), typeof(GameGrid),
            new PropertyMetadata(null, OnEngineChanged));

    public static readonly DependencyProperty CellColorProperty =
        DependencyProperty.Register(nameof(CellColor), typeof(Brush), typeof(GameGrid),
            new PropertyMetadata(Brushes.LimeGreen, OnVisualPropertyChanged));

    public static readonly DependencyProperty CellShapeProperty =
        DependencyProperty.Register(nameof(CellShape), typeof(string), typeof(GameGrid),
            new PropertyMetadata("Rectangle", OnVisualPropertyChanged));

    public static readonly DependencyProperty ZoomLevelProperty =
        DependencyProperty.Register(nameof(ZoomLevel), typeof(double), typeof(GameGrid),
            new PropertyMetadata(1.0, OnZoomChanged));

    public static readonly DependencyProperty RefreshTriggerProperty =
        DependencyProperty.Register(nameof(RefreshTrigger), typeof(int), typeof(GameGrid),
            new PropertyMetadata(0, OnRefreshTriggerChanged));

    public GameOfLifeEngine? Engine
    {
        get => (GameOfLifeEngine?)GetValue(EngineProperty);
        set => SetValue(EngineProperty, value);
    }

    public Brush CellColor
    {
        get => (Brush)GetValue(CellColorProperty);
        set => SetValue(CellColorProperty, value);
    }

    public string CellShape
    {
        get => (string)GetValue(CellShapeProperty);
        set => SetValue(CellShapeProperty, value);
    }

    public double ZoomLevel
    {
        get => (double)GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    public int RefreshTrigger
    {
        get => (int)GetValue(RefreshTriggerProperty);
        set => SetValue(RefreshTriggerProperty, value);
    }

    public GameGrid()
    {
        _backgroundVisual = new DrawingVisual();
        _cellsVisual = new DrawingVisual();
        _visualChildren = new VisualCollection(this) { _backgroundVisual, _cellsVisual };
        
        ClipToBounds = true;
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        SizeChanged += OnSizeChanged;
        
        // Update grid continuously
        CompositionTarget.Rendering += OnRendering;
    }

    protected override int VisualChildrenCount => _visualChildren?.Count ?? 0;

    protected override Visual GetVisualChild(int index)
    {
        if (_visualChildren == null)
            throw new ArgumentOutOfRangeException(nameof(index));
        return _visualChildren[index];
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (_engine != null && _cellsVisual != null)
        {
            // Throttle rendering - nie renderuj każdej klatki (60 FPS -> ~20-30 FPS jest wystarczające)
            _renderSkipCounter++;
            if (_renderSkipCounter < 2 && !_forceRender) // Renderuj co 2-3 klatki
                return;
            
            _renderSkipCounter = 0;

            // Aktualizuj gdy generacja się zmieni LUB gdy wymuszono render
            if (_lastGeneration != _engine.Generation || _forceRender)
            {
                _lastGeneration = _engine.Generation;
                _forceRender = false;
                RenderGrid();
            }
        }
    }

    private static void OnEngineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GameGrid grid)
        {
            grid._engine = e.NewValue as GameOfLifeEngine;
            grid._forceRender = true; // Wymuś odświeżenie
            grid.Dispatcher.BeginInvoke(new Action(() =>
            {
                grid.RebuildGrid();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GameGrid grid)
        {
            grid.RenderGrid();
        }
    }

    private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GameGrid grid)
        {
            grid.UpdateZoom();
        }
    }

    private static void OnRefreshTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GameGrid grid)
        {
            grid._forceRender = true;
            grid.RenderGrid();
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        CalculateCellSize();
    }

    private void CalculateCellSize()
    {
        if (_engine == null || ActualWidth == 0 || ActualHeight == 0)
            return;

        double baseCellWidth = ActualWidth / _engine.Width;
        double baseCellHeight = ActualHeight / _engine.Height;
        _cellSize = Math.Max(2, Math.Min(baseCellWidth, baseCellHeight));
        
        UpdateZoom();
        RenderGrid();
    }

    private void UpdateZoom()
    {
        if (_engine == null)
            return;

        var transform = new ScaleTransform(ZoomLevel, ZoomLevel);
        RenderTransform = transform;
        
        // Oblicz wymiary z walidacją
        double calculatedWidth = _engine.Width * _cellSize * ZoomLevel;
        double calculatedHeight = _engine.Height * _cellSize * ZoomLevel;
        
        // Ustaw tylko jeśli wartości są prawidłowe (> 0 i nie NaN)
        if (!double.IsNaN(calculatedWidth) && !double.IsInfinity(calculatedWidth) && calculatedWidth > 0)
        {
            Width = calculatedWidth;
        }
        
        if (!double.IsNaN(calculatedHeight) && !double.IsInfinity(calculatedHeight) && calculatedHeight > 0)
        {
            Height = calculatedHeight;
        }
    }

    public void ForceRender()
    {
        _forceRender = true;
    }

    private void RebuildGrid()
    {
        if (_engine == null || _cellsVisual == null || _backgroundVisual == null)
            return;

        _previousState = null; // Reset cache
        _lastGeneration = -1; // Reset generation counter

        // Renderuj tło
        using (DrawingContext dc = _backgroundVisual.RenderOpen())
        {
            dc.DrawRectangle(Brushes.Black, null, new Rect(0, 0, 
                _engine.Width * _cellSize, _engine.Height * _cellSize));
        }

        CalculateCellSize();
        RenderGrid();
    }

    private void RenderGrid()
    {
        if (_engine == null || _cellsVisual == null)
            return;

        // Sprawdź czy mamy poprzedni stan do porównania
        bool hasValidPreviousState = _previousState != null && 
                                      _previousState.GetLength(0) == _engine.Width && 
                                      _previousState.GetLength(1) == _engine.Height;

        using (DrawingContext dc = _cellsVisual.RenderOpen())
        {
            // Nie rysuj tła tutaj - jest na osobnej warstwie!
            
            if (!hasValidPreviousState)
            {
                // Pierwszy render - narysuj wszystkie żywe komórki
                RenderAllCells(dc);
            }
            else
            {
                // Optymalizacja - rysuj tylko zmienione komórki!
                RenderAllCells(dc); // Niestety DrawingContext wymaga pełnego redraw
            }

            // Update cache
            _previousState = _engine.GetStateCopy();
        }
    }

    private void RenderAllCells(DrawingContext dc)
    {
        if (_engine == null) return;

        // Batch rendering - zbierz wszystkie prostokąty i narysuj je jednocześnie
        if (CellShape == "Rectangle")
        {
            var geometry = new GeometryGroup();
            for (int x = 0; x < _engine.Width; x++)
            {
                for (int y = 0; y < _engine.Height; y++)
                {
                    if (_engine.GetCell(x, y))
                    {
                        double posX = x * _cellSize;
                        double posY = y * _cellSize;
                        double size = _cellSize - 0.5;
                        geometry.Children.Add(new RectangleGeometry(new Rect(posX, posY, size, size)));
                    }
                }
            }
            if (geometry.Children.Count > 0)
                dc.DrawGeometry(CellColor, null, geometry);
        }
        else
        {
            // Dla kształtów niestandartowych rysuj pojedynczo
            for (int x = 0; x < _engine.Width; x++)
            {
                for (int y = 0; y < _engine.Height; y++)
                {
                    if (_engine.GetCell(x, y))
                    {
                        DrawCell(dc, x, y);
                    }
                }
            }
        }
    }

    private void DrawCell(DrawingContext dc, int x, int y)
    {
        double posX = x * _cellSize;
        double posY = y * _cellSize;
        double size = _cellSize - 0.5;

        if (CellShape == "Ellipse")
        {
            double centerX = posX + size / 2;
            double centerY = posY + size / 2;
            double radius = size / 2;
            dc.DrawEllipse(CellColor, null, new Point(centerX, centerY), radius, radius);
        }
        else if (CellShape == "RoundedRectangle")
        {
            dc.DrawRoundedRectangle(CellColor, null, 
                new Rect(posX, posY, size, size), 2, 2);
        }
        else
        {
            dc.DrawRectangle(CellColor, null, 
                new Rect(posX, posY, size, size));
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        HandleMouseInteraction(e.GetPosition(this));
        CaptureMouse();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
        {
            HandleMouseInteraction(e.GetPosition(this));
        }
        else if (e.LeftButton == MouseButtonState.Released)
        {
            ReleaseMouseCapture();
        }
    }

    private void HandleMouseInteraction(Point position)
    {
        if (_engine == null)
            return;
            
        if (_viewModel == null)
        {
            _viewModel = DataContext as MainViewModel;
            if (_viewModel == null) return;
        }

        int x = (int)(position.X / (_cellSize * ZoomLevel));
        int y = (int)(position.Y / (_cellSize * ZoomLevel));

        if (x >= 0 && x < _engine.Width && y >= 0 && y < _engine.Height)
        {
            _viewModel.ToggleCell(x, y);
            RenderGrid();
        }
    }
}

