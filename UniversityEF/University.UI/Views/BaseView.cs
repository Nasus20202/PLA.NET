using Terminal.Gui;

namespace University.UI.Views;

public abstract class BaseView : FrameView
{
    protected readonly IServiceProvider ServiceProvider;

    protected BaseView(IServiceProvider serviceProvider, string title)
        : base(title)
    {
        ServiceProvider = serviceProvider;
        X = 0;
        Y = 0;
        Width = Dim.Fill();
        Height = Dim.Fill();
    }

    public abstract Task LoadDataAsync();
}
