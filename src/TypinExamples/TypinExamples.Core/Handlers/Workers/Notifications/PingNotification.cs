﻿namespace TypinExamples.Core.Handlers.Workers.Notifications
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    public class WorkerPingNotification : IWorkerNotification
    {

    }

    public class WorkerPong1 : IWorkerNotificationHandler<WorkerPingNotification>
    {
        public Task Handle(WorkerPingNotification notification, CancellationToken cancellationToken)
        {
            Debug.WriteLine("Pong 1");
            return Task.CompletedTask;
        }
    }

    public class WorkerPong2 : IWorkerNotificationHandler<WorkerPingNotification>
    {
        public Task Handle(WorkerPingNotification notification, CancellationToken cancellationToken)
        {
            Debug.WriteLine("Pong 2");
            return Task.CompletedTask;
        }
    }
}
