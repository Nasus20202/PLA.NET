using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameOfLife.Models;
using GameOfLife.ViewModels;

namespace GameOfLife.Controls;

/// <summary>
///     Custom control for rendering and interacting with the Game of Life grid
///     Optimized version using DrawingVisual for low memory usage with viewport culling
/// </summary>
public class GameGrid : FrameworkElement
{
    private const double BaseCellSize = 10; // Fixed base cell size

    public static readonly DependencyProperty EngineProperty = DependencyProperty.Register(
        nameof(Engine),
        typeof(GameOfLifeEngine),
        typeof(GameGrid),
        new PropertyMetadata(null, OnEngineChanged)
    );

    public static readonly DependencyProperty CellColorProperty = DependencyProperty.Register(
        nameof(CellColor),
        typeof(Brush),
        typeof(GameGrid),
        new PropertyMetadata(Brushes.LimeGreen, OnVisualPropertyChanged)
    );

    public static readonly DependencyProperty CellShapeProperty = DependencyProperty.Register(
        nameof(CellShape),
        typeof(string),
        typeof(GameGrid),
        new PropertyMetadata("Rectangle", OnVisualPropertyChanged)
    );

    public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
        nameof(ZoomLevel),
        typeof(double),
        typeof(GameGrid),
        new PropertyMetadata(1.0, OnZoomChanged)
    );

    public static readonly DependencyProperty RefreshTriggerProperty = DependencyProperty.Register(
        nameof(RefreshTrigger),
        typeof(int),
        typeof(GameGrid),
        new PropertyMetadata(0, OnRefreshTriggerChanged)
    );

    public static readonly DependencyProperty ColoringModelProperty = DependencyProperty.Register(
        nameof(ColoringModel),
        typeof(IColoringModel),
        typeof(GameGrid),
        new PropertyMetadata(null, OnColoringModelChanged)
    );

    private readonly DrawingVisual? _backgroundVisual;
    private readonly DrawingVisual? _cellsVisual;
    private readonly VisualCollection? _visualChildren;
    private GameOfLifeEngine? _engine;
    private bool _forceRender;
    private long _lastGeneration = -1;
    private int _renderSkipCounter;
    private MainViewModel? _viewModel;

    public GameGrid()
    {
        _backgroundVisual = new DrawingVisual();
        _cellsVisual = new DrawingVisual();
        _visualChildren = new VisualCollection(this) { _backgroundVisual, _cellsVisual };

        ClipToBounds = true;
        Focusable = true; // Enable keyboard/mouse wheel focus
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        MouseWheel += OnMouseWheel;
        SizeChanged += OnSizeChanged;
        Loaded += OnLoaded;

        // Update grid continuously
        CompositionTarget.Rendering += OnRendering;
    }

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

    public IColoringModel? ColoringModel
    {
        get => (IColoringModel?)GetValue(ColoringModelProperty);
        set => SetValue(ColoringModelProperty, value);
    }

    protected override int VisualChildrenCount => _visualChildren?.Count ?? 0;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Find and attach to parent ScrollViewer's ScrollChanged event
        var parent = VisualTreeHelper.GetParent(this);
        while (parent != null && parent is not ScrollViewer)
            parent = VisualTreeHelper.GetParent(parent);

        if (parent is ScrollViewer scrollViewer)
            scrollViewer.ScrollChanged += OnScrollChanged;
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // Force re-render when scrolling to update visible cells
        _forceRender = true;
    }

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
            // Throttle rendering - render every 2-3 frames instead of 60 FPS
            _renderSkipCounter++;
            if (_renderSkipCounter < 2 && !_forceRender)
                return;

            _renderSkipCounter = 0;

            // Update when generation changes OR when forced
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
            grid._forceRender = true;
            grid.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    grid.RebuildGrid();
                }),
                DispatcherPriority.Loaded
            );
        }
    }

    private static void OnVisualPropertyChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is GameGrid grid)
            grid._forceRender = true;
    }

    private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GameGrid grid)
            grid.UpdateZoom();
    }

    private static void OnRefreshTriggerChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is GameGrid grid)
            grid._forceRender = true;
    }

    private static void OnColoringModelChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is GameGrid grid)
            grid._forceRender = true;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        _forceRender = true;
    }

    private void UpdateDimensions()
    {
        if (_engine == null)
            return;

        // Set natural size based on engine dimensions and base cell size
        Width = _engine.Width * BaseCellSize;
        Height = _engine.Height * BaseCellSize;
    }

    private Rect GetVisibleBounds()
    {
        // Find the parent ScrollViewer to get viewport bounds
        var parent = VisualTreeHelper.GetParent(this);
        while (parent != null && parent is not ScrollViewer)
            parent = VisualTreeHelper.GetParent(parent);

        if (parent is ScrollViewer scrollViewer)
        {
            // Get the viewport in the ScrollViewer's coordinate space (zoomed space)
            var viewportWidth = scrollViewer.ViewportWidth;
            var viewportHeight = scrollViewer.ViewportHeight;
            var horizontalOffset = scrollViewer.HorizontalOffset;
            var verticalOffset = scrollViewer.VerticalOffset;

            // The ScrollViewer offsets and viewport are in the TRANSFORMED (zoomed) coordinate space
            // We need to convert back to the untransformed space by dividing by zoom level
            var zoom = ZoomLevel;

            return new Rect(
                horizontalOffset / zoom,
                verticalOffset / zoom,
                viewportWidth / zoom,
                viewportHeight / zoom
            );
        }

        // If no ScrollViewer found, return entire grid
        return new Rect(0, 0, Width, Height);
    }

    private void UpdateZoom()
    {
        if (_engine == null)
            return;

        // Update base dimensions first
        UpdateDimensions();

        // Apply zoom transformation using LayoutTransform for proper ScrollViewer interaction
        // LayoutTransform affects the layout system, so ScrollViewer will see the correct size
        LayoutTransform = new ScaleTransform(ZoomLevel, ZoomLevel);

        // Force re-render with new zoom
        _forceRender = true;
    }

    private void RebuildGrid()
    {
        if (_engine == null || _cellsVisual == null || _backgroundVisual == null)
            return;

        _lastGeneration = -1;

        // Update dimensions
        UpdateDimensions();

        // Render background
        using (var dc = _backgroundVisual.RenderOpen())
        {
            dc.DrawRectangle(
                Brushes.Black,
                null,
                new Rect(0, 0, _engine.Width * BaseCellSize, _engine.Height * BaseCellSize)
            );
        }

        // Apply zoom and render
        UpdateZoom();
    }

    private void RenderGrid()
    {
        if (_engine == null || _cellsVisual == null)
            return;

        using (var dc = _cellsVisual.RenderOpen())
        {
            // Background is on a separate layer, just draw the cells
            RenderVisibleCells(dc);
        }
    }

    private void RenderVisibleCells(DrawingContext dc)
    {
        if (_engine == null)
            return;

        // Get visible viewport bounds
        var visibleBounds = GetVisibleBounds();

        // Calculate which cells are visible (add margin for smooth scrolling)
        var margin = 5; // Extra cells to render beyond viewport
        var startX = Math.Max(0, (int)(visibleBounds.Left / BaseCellSize) - margin);
        var endX = Math.Min(_engine.Width, (int)(visibleBounds.Right / BaseCellSize) + margin + 1);
        var startY = Math.Max(0, (int)(visibleBounds.Top / BaseCellSize) - margin);
        var endY = Math.Min(
            _engine.Height,
            (int)(visibleBounds.Bottom / BaseCellSize) + margin + 1
        );

        // Determine if we should use ColoringModel (only if it's not Standard coloring)
        var useColoringModel = ColoringModel != null && ColoringModel.Name != "Standard";

        // Batch rendering for single-color cells (Rectangle shape with CellColor and no special coloring)
        if (CellShape == "Rectangle" && !useColoringModel)
        {
            var geometry = new GeometryGroup();
            for (var x = startX; x < endX; x++)
            for (var y = startY; y < endY; y++)
                if (_engine.GetCell(x, y))
                {
                    var posX = x * BaseCellSize;
                    var posY = y * BaseCellSize;
                    var size = BaseCellSize - 0.5;
                    geometry.Children.Add(new RectangleGeometry(new Rect(posX, posY, size, size)));
                }

            if (geometry.Children.Count > 0)
                dc.DrawGeometry(CellColor, null, geometry);
        }
        else
        {
            // For color-varying cells or non-standard shapes, draw individually
            for (var x = startX; x < endX; x++)
            for (var y = startY; y < endY; y++)
                if (_engine.GetCell(x, y))
                {
                    Brush cellBrush;
                    if (useColoringModel && ColoringModel != null)
                    {
                        var neighbors = _engine.GetNeighborCount(x, y);
                        var color = ColoringModel.GetCellColor(x, y, true, 0, neighbors);
                        cellBrush = new SolidColorBrush(color);
                    }
                    else
                    {
                        cellBrush = CellColor;
                    }

                    DrawCell(dc, x, y, cellBrush);
                }
        }
    }

    private void DrawCell(DrawingContext dc, int x, int y, Brush? cellBrush = null)
    {
        cellBrush ??= CellColor;
        var posX = x * BaseCellSize;
        var posY = y * BaseCellSize;
        var size = BaseCellSize - 0.5;

        if (CellShape == "Ellipse")
        {
            var centerX = posX + size / 2;
            var centerY = posY + size / 2;
            var radius = size / 2;
            dc.DrawEllipse(cellBrush, null, new Point(centerX, centerY), radius, radius);
        }
        else if (CellShape == "RoundedRectangle")
        {
            dc.DrawRoundedRectangle(cellBrush, null, new Rect(posX, posY, size, size), 2, 2);
        }
        else
        {
            dc.DrawRectangle(cellBrush, null, new Rect(posX, posY, size, size));
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
            HandleMouseInteraction(e.GetPosition(this));
        else if (e.LeftButton == MouseButtonState.Released)
            ReleaseMouseCapture();
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_viewModel == null)
        {
            _viewModel = DataContext as MainViewModel;
            if (_viewModel == null)
                return;
        }

        // Calculate zoom change based on wheel delta
        // Positive delta = zoom in, negative = zoom out
        var zoomDelta = e.Delta > 0 ? 0.1 : -0.1;
        var newZoom = _viewModel.ZoomLevel + zoomDelta;

        // Clamp zoom between 0.5 and 5.0 for reasonable limits
        newZoom = Math.Max(0.5, Math.Min(5.0, newZoom));

        // Update the view model's zoom level
        _viewModel.ZoomLevel = newZoom;

        // Mark as handled so parent controls don't also process it
        e.Handled = true;
    }

    private void HandleMouseInteraction(Point position)
    {
        if (_engine == null)
            return;

        if (_viewModel == null)
        {
            _viewModel = DataContext as MainViewModel;
            if (_viewModel == null)
                return;
        }

        // Since we use LayoutTransform, the position is already in the transformed coordinate space
        // We just need to divide by BaseCellSize
        var x = (int)(position.X / BaseCellSize);
        var y = (int)(position.Y / BaseCellSize);

        if (x >= 0 && x < _engine.Width && y >= 0 && y < _engine.Height)
        {
            _viewModel.ToggleCell(x, y);
            _forceRender = true;
        }
    }
}
