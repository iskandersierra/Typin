﻿namespace Typin.Directives
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Typin.Attributes;
    using Typin.Console;
    using Typin.Modes;

    /// <summary>
    /// If application runs in interactive mode, this [..] directive can be used to reset current scope to default (global scope).
    /// <example>
    ///             > [>] cmd1 sub
    ///     cmd1 sub> list
    ///     cmd1 sub> [..]
    ///             >
    /// </example>
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Directive(BuiltInDirectives.ScopeReset, Description = "Resets the scope to default value.", SupportedModes = new[] { typeof(InteractiveMode) })]
    public sealed class ScopeResetDirective : IDirective
    {
        private readonly InteractiveModeSettings _settings;

        /// <inheritdoc/>
        public bool ContinueExecution => false;

        /// <summary>
        /// Initializes an instance of <see cref="ScopeResetDirective"/>.
        /// </summary>
        public ScopeResetDirective(InteractiveModeSettings settings)
        {
            _settings = settings;
        }

        /// <inheritdoc/>
        public ValueTask HandleAsync(IConsole console)
        {
            _settings.Scope = string.Empty;

            return default;
        }
    }
}
