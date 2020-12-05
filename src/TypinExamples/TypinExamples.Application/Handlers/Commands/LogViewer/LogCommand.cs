﻿namespace TypinExamples.Application.Handlers.Commands.Terminal
{
    using System.Threading;
    using System.Threading.Tasks;
    using TypinExamples.Application.Services;
    using TypinExamples.Application.Services.TypinWeb;
    using TypinExamples.Infrastructure.WebWorkers.Abstractions;
    using TypinExamples.Infrastructure.WebWorkers.Abstractions.Messaging;
    using TypinExamples.Infrastructure.WebWorkers.Abstractions.Payloads;

    public sealed class LogCommand : ICommand
    {
        public string? TerminalId { get; init; }
        public string? Value { get; init; }

        public class Handler : ICommandHandler<LogCommand>
        {
            private readonly ITerminalRepository _terminalRepository;

            public Handler(ITerminalRepository terminalRepository)
            {
                _terminalRepository = terminalRepository;
            }

            public async ValueTask<CommandFinished> HandleAsync(LogCommand request, IWorker worker, CancellationToken cancellationToken)
            {
                if (request.TerminalId is string id &&
                    request.Value is string value &&
                    _terminalRepository.GetOrDefault(id) is IWebTerminal webTerminal)
                {
                    await webTerminal.WriteLineAsync(value);
                }

                return CommandFinished.Instance;
            }
        }
    }
}
