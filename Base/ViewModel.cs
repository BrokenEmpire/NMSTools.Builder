using System;
using System.Windows;

namespace NMSTools.Builder.Base
{
    public abstract class ViewModel : DependencyObject, IDisposable
    {
        public abstract void Dispose();
    }
}
