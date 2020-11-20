﻿using BlazorWorker.Core.SimpleInstanceService;

namespace BlazorWorker.Core.CoreInstanceService
{
    public static class SimpleInstanceServiceExtension
    {
        public static ICoreInstanceService CreateCoreInstanceService(this IWorker source)
        {
            return new CoreInstanceService(new SimpleInstanceServiceProxy(source));
        }
    }
}
