namespace Audio.MAUI;

public class MediaSeekBar : ContentView
{
    public static readonly BindableProperty PointerStrokeColorProperty = BindableProperty.Create(nameof(PointerStrokeColor), typeof(Color), typeof(MediaSeekBar), Colors.White, propertyChanged: Invalidate);
    public static readonly BindableProperty PointerStrokeProperty = BindableProperty.Create(nameof(PointerStroke), typeof(float), typeof(MediaSeekBar), 2f, propertyChanged: Invalidate);
    public static readonly BindableProperty PointerFillColorProperty = BindableProperty.Create(nameof(PointerFillColor), typeof(Color), typeof(MediaSeekBar), Colors.White, propertyChanged: Invalidate);
    public static readonly BindableProperty PointerDiameterProperty = BindableProperty.Create(nameof(PointerDiameter), typeof(float), typeof(MediaSeekBar), 15f, propertyChanged: Invalidate);
    public static readonly BindableProperty StrokeProperty = BindableProperty.Create(nameof(Stroke), typeof(float), typeof(MediaSeekBar), 2f, propertyChanged: Invalidate);
    public static readonly BindableProperty ElapsedStrokeColorProperty = BindableProperty.Create(nameof(ElapsedStrokeColor), typeof(Color), typeof(MediaSeekBar), Colors.White, propertyChanged: Invalidate);
    public static readonly BindableProperty PendingStrokeColorProperty = BindableProperty.Create(nameof(PendingStrokeColor), typeof(Color), typeof(MediaSeekBar), Colors.White, propertyChanged: Invalidate);
    public static readonly BindableProperty ElapsedFillColorProperty = BindableProperty.Create(nameof(ElapsedFillColor), typeof(Color), typeof(MediaSeekBar), Colors.White, propertyChanged: Invalidate);
    public static readonly BindableProperty PendingFillColorProperty = BindableProperty.Create(nameof(PendingFillColor), typeof(Color), typeof(MediaSeekBar), Colors.Transparent, propertyChanged: Invalidate);
    public static readonly BindableProperty BarHeightProperty = BindableProperty.Create(nameof(BarHeight), typeof(float), typeof(MediaSeekBar), 9f, propertyChanged: Invalidate);
    public static readonly BindableProperty MinValueProperty = BindableProperty.Create(nameof(MinValue), typeof(TimeSpan), typeof(MediaSeekBar), TimeSpan.Zero, propertyChanged: Invalidate);
    public static readonly BindableProperty MaxValueProperty = BindableProperty.Create(nameof(MaxValue), typeof(TimeSpan), typeof(MediaSeekBar), TimeSpan.Zero, propertyChanged: Invalidate);
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(TimeSpan), typeof(MediaSeekBar), TimeSpan.Zero, propertyChanged: Invalidate);

    /// <summary>
    /// Set the stroke color for the selector pointer.
    /// </summary>
    public Color PointerStrokeColor
    {
        get { return (Color)GetValue(PointerStrokeColorProperty); }
        set { SetValue(PointerStrokeColorProperty, value); }
    }
    /// <summary>
    /// Set the stroke size for the selector pointer.
    /// </summary>
    public float PointerStroke
    {
        get { return (float)GetValue(PointerStrokeProperty); }
        set { SetValue(PointerStrokeProperty, value); }
    }
    /// <summary>
    /// Set the stroke color for the selector pointer.
    /// </summary>
    public Color PointerFillColor
    {
        get { return (Color)GetValue(PointerFillColorProperty); }
        set { SetValue(PointerFillColorProperty, value); }
    }
    /// <summary>
    /// Set the diameter for the selector pointer.
    /// </summary>
    public float PointerDiameter
    {
        get { return (float)GetValue(PointerDiameterProperty); }
        set { SetValue(PointerDiameterProperty, value); }
    }
    /// <summary>
    /// Set the stroke size for the bar.
    /// </summary>
    public float Stroke
    {
        get { return (float)GetValue(StrokeProperty); }
        set { SetValue(StrokeProperty, value); }
    }
    /// <summary>
    /// Set the stroke color for the elapsed section of the bar.
    /// </summary>
    public Color ElapsedStrokeColor
    {
        get { return (Color)GetValue(ElapsedStrokeColorProperty); }
        set { SetValue(ElapsedStrokeColorProperty, value); }
    }
    /// <summary>
    /// Set the fill color for the elapsed section of the bar.
    /// </summary>
    public Color ElapsedFillColor
    {
        get { return (Color)GetValue(ElapsedFillColorProperty); }
        set { SetValue(ElapsedFillColorProperty, value); }
    }
    /// <summary>
    /// Set the stroke color for the pending section of the bar.
    /// </summary>
    public Color PendingStrokeColor
    {
        get { return (Color)GetValue(PendingStrokeColorProperty); }
        set { SetValue(PendingStrokeColorProperty, value); }
    }
    /// <summary>
    /// Set the fill color for the pending section of the bar.
    /// </summary>
    public Color PendingFillColor
    {
        get { return (Color)GetValue(PendingFillColorProperty); }
        set { SetValue(PendingFillColorProperty, value); }
    }
    /// <summary>
    /// Set the bar height.
    /// </summary>
    public float BarHeight
    {
        get { return (float)GetValue(BarHeightProperty); }
        set { SetValue(BarHeightProperty, value); }
    }
    /// <summary>
    /// Set the minimun traking value.
    /// </summary>
    public TimeSpan MinValue
    {
        get { return (TimeSpan)GetValue(MinValueProperty); }
        set { SetValue(MinValueProperty, value); }
    }
    /// <summary>
    /// Set the maximun traking value.
    /// </summary>
    public TimeSpan MaxValue
    {
        get { return (TimeSpan)GetValue(MaxValueProperty); }
        set { SetValue(MaxValueProperty, value); }
    }
    /// <summary>
    /// Set the current traking value.
    /// </summary>
    public TimeSpan Value
    {
        get { return (TimeSpan)GetValue(MaxValueProperty); }
        set { SetValue(MaxValueProperty, value); }
    }
    private readonly GraphicsDrawable drawable;
	private readonly GraphicsView view;
	public MediaSeekBar()
	{
		drawable = new(this);
		view = new() { Drawable = drawable, VerticalOptions = LayoutOptions.Fill, HorizontalOptions = LayoutOptions.Fill };
        PanGestureRecognizer panGesture = new();
        DragGestureRecognizer dragGesture = new();
        PinchGestureRecognizer pinchGesture = new();
        TapGestureRecognizer tapGesture = new();
        pinchGesture.PinchUpdated += PinchGesture_PinchUpdated;
        dragGesture.DragStarting += DragGesture_DragStarting;
        panGesture.PanUpdated += PanGesture_PanUpdated;
        view.GestureRecognizers.Add(panGesture);
        Content = view;
        SizeChanged += MediaSeekBar_SizeChanged;
        
	}

    private void PinchGesture_PinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        
    }

    private void DragGesture_DragStarting(object sender, DragStartingEventArgs e)
    {
        
    }

    private void PanGesture_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                
                break;
            case GestureStatus.Running:
                
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                break;
        }
    }
    private void MediaSeekBar_SizeChanged(object sender, EventArgs e)
    {
        if (MaxValue.TotalMilliseconds < MinValue.TotalMilliseconds)
            MaxValue = TimeSpan.FromMicroseconds(MinValue.TotalMilliseconds);
        if (Value.TotalMilliseconds > MaxValue.TotalMilliseconds || Value.TotalMilliseconds < MinValue.TotalMilliseconds)
            Value = TimeSpan.FromMicroseconds(Math.Clamp(Value.TotalMilliseconds, MinValue.TotalMilliseconds, MaxValue.TotalMilliseconds));
        view.Invalidate();
    }
    private static void Invalidate(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue != newValue && bindable is MediaSeekBar control)
        {
            control.view.Invalidate();
        }
    }
    private class GraphicsDrawable : IDrawable
    {
        MediaSeekBar mediaSeekBar;
        public GraphicsDrawable(MediaSeekBar mediaSeekBar)
        {
            this.mediaSeekBar = mediaSeekBar;
        }
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            double width = mediaSeekBar.Width;
            double height = mediaSeekBar.Height;
            double minValue = mediaSeekBar.MinValue.TotalMilliseconds;
            double maxValue = mediaSeekBar.MaxValue.TotalMilliseconds;
            double value = Math.Clamp(mediaSeekBar.Value.TotalMilliseconds, minValue, maxValue);
            if (width >= mediaSeekBar.PointerDiameter * 2 && height >= mediaSeekBar.PointerDiameter)
            {
                float barHeight = Math.Min(mediaSeekBar.BarHeight, mediaSeekBar.PointerDiameter);
                canvas.StrokeSize = mediaSeekBar.Stroke;
                canvas.StrokeColor = mediaSeekBar.PendingStrokeColor;
                canvas.FillColor = mediaSeekBar.PendingFillColor;
                canvas.DrawRoundedRectangle(mediaSeekBar.PointerDiameter / 2, ((float)height - barHeight) / 2, (float)width - mediaSeekBar.PointerDiameter, barHeight, barHeight / 2);
                float pointerX = mediaSeekBar.PointerDiameter / 2;
                if (value > minValue)
                {
                    double total = maxValue - minValue;
                    var inc = (width - mediaSeekBar.PointerDiameter) / total;
                    pointerX += (float)(value * inc);
                    canvas.StrokeColor = mediaSeekBar.ElapsedStrokeColor;
                    canvas.FillColor = mediaSeekBar.ElapsedFillColor;
                    canvas.DrawRoundedRectangle(mediaSeekBar.PointerDiameter / 2, ((float)height - barHeight) / 2, (float)(value * inc), barHeight, barHeight / 2);
                }
                canvas.StrokeColor = mediaSeekBar.PointerStrokeColor;
                canvas.FillColor = mediaSeekBar.PointerFillColor;
                canvas.StrokeSize = mediaSeekBar.PointerStroke;
                canvas.FillCircle(pointerX, (float)height / 2, mediaSeekBar.PointerDiameter / 2);
                canvas.DrawCircle(pointerX, (float)height / 2, mediaSeekBar.PointerDiameter / 2);

            }
        }
    }
}