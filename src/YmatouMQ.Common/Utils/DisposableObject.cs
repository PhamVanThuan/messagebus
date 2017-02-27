namespace YmatouMQ.Common.Utils
{
    using System;
    using System.Diagnostics;

    public abstract class DisposableObject : IDisposable
    {
        //标记是否已释放资源      
        private bool disposed;

        [DebuggerStepThrough]
        ~DisposableObject()
        {
            Dispose(false);
        }

        [DebuggerStepThrough]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [DebuggerStepThrough]
        protected abstract void InternalDispose();

        [DebuggerStepThrough]
        private void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                InternalDispose();
            }
            disposed = true;
        }
    }
}
