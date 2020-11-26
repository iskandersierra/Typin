﻿namespace TypinExamples.Infrastructure.WebWorkers.WorkerCore
{
    using System;
    using TypinExamples.Infrastructure.WebWorkers.WorkerCore.WebAssemblyBindingsProxy;

    // Serves as a wrapper around a JSObject.
    internal class DOMObject : IDisposable
    {

        public JSObject ManagedJSObject { get; private set; }

        public DOMObject(JSObject jsobject)
        {
            ManagedJSObject = jsobject ?? throw new ArgumentNullException(nameof(jsobject));
        }

        public DOMObject(string globalName) : this(new JSObject(Runtime.GetGlobalObject(globalName)))
        { }

        public object Invoke(string method, params object[] args)
        {
            return ManagedJSObject.Invoke(method, args);
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {

            if (disposing)
            {

                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            ManagedJSObject?.Dispose();
            ManagedJSObject = null;
        }

    }
}
