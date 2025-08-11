using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

//
// 托管资源
// 非托管资源释放基类

namespace UtilsShared
{
    public abstract class HDisposable : IDisposable, INotifyPropertyChanged
    {
        // IDisposable 相关
        public bool IsDisposed { get; private set; } = false;

        // INotifyPropertyChanged 相关
        public event PropertyChangedEventHandler? PropertyChanged = null;

        public void Dispose()
        {
            InterDispose(true);
            GC.SuppressFinalize(this);
        }

        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        ~HDisposable()
        {
            InterDispose(false);
        }

        protected bool SetProp<T>(ref T field, T value, Action<T, T> onChanged = null, [CallerMemberName] string propName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            T old = field;
            field = value;
            onChanged?.Invoke(old, value);
            OnPropertyChanged(propName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void InterDispose(bool disposing)
        {
            if (IsDisposed)
                return;

            try
            {
                if (disposing)
                    OnDisposeManaged();

                OnDisposeUnmanaged();
            }
            finally
            {
                IsDisposed = true;
            }
        }

        protected abstract void OnDisposeManaged();

        protected virtual void OnDisposeUnmanaged()
        { }
    }
}