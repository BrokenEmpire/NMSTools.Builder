using System.Windows;
using System.Windows.Input;

namespace NMSTools.Builder.Base
{
    public abstract class WindowView : Window
    {
        protected virtual void WindowView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        protected virtual void WindowView_Closed(object sender, System.EventArgs e)
        {
            Closed -= WindowView_Closed;
            MouseLeftButtonDown -= WindowView_MouseLeftButtonDown;

            if (DataContext is ViewModel obj && obj != null)
            {
                obj.Dispose();
                DataContext = null;
            }               
        }

        protected WindowView()
        {
            Closed += WindowView_Closed;
            MouseLeftButtonDown += WindowView_MouseLeftButtonDown;
        }
    }

    public abstract class WindowView<T> : WindowView where T : ViewModel, new()
    {
        protected WindowView(T viewModel = default) => DataContext = viewModel ?? new T();
    }
}
