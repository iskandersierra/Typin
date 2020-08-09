using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CliFx.Domain;
using CliFx.Exceptions;
using CliFx.Internal.Extensions;

namespace CliFx
{
    /// <summary>
    /// Builds an instance of <see cref="CliApplication"/>.
    /// </summary>
    public partial class CliApplicationBuilder
    {
        //Directives and commands settings
        private readonly HashSet<Type> _commandTypes = new HashSet<Type>();
        private readonly List<string> _customDirectives = new List<string>();

        private bool _isDebugModeAllowed = true;
        private bool _isPreviewModeAllowed = true;

        //Metadata settings
        private string? _title;
        private string? _executableName;
        private string? _versionText;
        private string? _description;

        //Exceptions
        private ICliExceptionHandler? _exceptionHandler;

        // Dependecy injection and type activation
        private IConsole? _console;
        private ITypeActivator? _typeActivator;
        private Func<ICliContext, IConsole, Func<Type, object>>? _buildServiceProvider;

        //Interactive mode settings
        private bool _isInteractiveModeAllowed = false;

        #region Directives and commands
        /// <summary>
        /// Add custom directive.
        /// </summary>
        public CliApplicationBuilder AddDirective(string directiveName)
        {
            _customDirectives.Add(directiveName.Trim('[', ']'));

            return this;
        }

        /// <summary>
        /// Add custom directive.
        /// </summary>
        public CliApplicationBuilder AddDirectives(params string[] directivesNames)
        {
            foreach (var directiveName in directivesNames)
                AddDirective(directiveName);

            return this;
        }

        /// <summary>
        /// Adds a command of specified type to the application.
        /// </summary>
        public CliApplicationBuilder AddCommand(Type commandType)
        {
            _commandTypes.Add(commandType);

            return this;
        }

        /// <summary>
        /// Adds multiple commands to the application.
        /// </summary>
        public CliApplicationBuilder AddCommands(IEnumerable<Type> commandTypes)
        {
            foreach (var commandType in commandTypes)
                AddCommand(commandType);

            return this;
        }

        /// <summary>
        /// Adds commands from the specified assembly to the application.
        /// Only adds public valid command types.
        /// </summary>
        public CliApplicationBuilder AddCommandsFrom(Assembly commandAssembly)
        {
            foreach (var commandType in commandAssembly.ExportedTypes.Where(CommandSchema.IsCommandType))
                AddCommand(commandType);

            return this;
        }

        /// <summary>
        /// Adds commands from the specified assemblies to the application.
        /// Only adds public valid command types.
        /// </summary>
        public CliApplicationBuilder AddCommandsFrom(IEnumerable<Assembly> commandAssemblies)
        {
            foreach (var commandAssembly in commandAssemblies)
                AddCommandsFrom(commandAssembly);

            return this;
        }

        /// <summary>
        /// Adds commands from the calling assembly to the application.
        /// Only adds public valid command types.
        /// </summary>
        public CliApplicationBuilder AddCommandsFromThisAssembly()
        {
            return AddCommandsFrom(Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// Specifies whether debug mode (enabled with [debug] directive) is allowed in the application.
        /// </summary>
        public CliApplicationBuilder AllowDebugMode(bool isAllowed = true)
        {
            _isDebugModeAllowed = isAllowed;
            return this;
        }

        /// <summary>
        /// Specifies whether preview mode (enabled with [preview] directive) is allowed in the application.
        /// </summary>
        public CliApplicationBuilder AllowPreviewMode(bool isAllowed = true)
        {
            _isPreviewModeAllowed = isAllowed;
            return this;
        }
        #endregion

        #region Metadata
        /// <summary>
        /// Sets application title, which appears in the help text.
        /// </summary>
        public CliApplicationBuilder UseTitle(string title)
        {
            _title = title;
            return this;
        }

        /// <summary>
        /// Sets application executable name, which appears in the help text.
        /// </summary>
        public CliApplicationBuilder UseExecutableName(string executableName)
        {
            _executableName = executableName;
            return this;
        }

        /// <summary>
        /// Sets application version text, which appears in the help text and when the user requests version information.
        /// </summary>
        public CliApplicationBuilder UseVersionText(string versionText)
        {
            _versionText = versionText;
            return this;
        }

        /// <summary>
        /// Sets application description, which appears in the help text.
        /// </summary>
        public CliApplicationBuilder UseDescription(string? description)
        {
            _description = description;
            return this;
        }
        #endregion

        #region Dependecy injection and type activation
        /// <summary>
        /// Configures the application to use the specified implementation of <see cref="IConsole"/>.
        /// </summary>
        public CliApplicationBuilder UseConsole(IConsole console)
        {
            _console = console;
            return this;
        }

        /// <summary>
        /// Configures the application to use the specified implementation of <see cref="IConsole"/>.
        /// </summary>
        public CliApplicationBuilder UseConsole<T>() where T : class, IConsole, new()
        {
            _console = new T();
            return this;
        }

        /// <summary>
        /// Configures the application to use the specified implementation of <see cref="ITypeActivator"/>.
        /// </summary>
        public CliApplicationBuilder UseTypeActivator(ITypeActivator typeActivator)
        {
            _buildServiceProvider = null;
            _typeActivator = typeActivator;

            return this;
        }

        /// <summary>
        /// Configures the application to use the specified function for activating types.
        /// </summary>
        public CliApplicationBuilder UseTypeActivator(Func<Type, object> typeActivator)
        {
            return UseTypeActivator(new DelegateTypeActivator(typeActivator));
        }

        /// <summary>
        /// Configures the application to use the specified implementation of <see cref="ITypeActivator"/>.
        /// </summary>
        public CliApplicationBuilder UseTypeActivator(Func<ICliContext, IConsole, Func<Type, object>> buildServiceProvider)
        {
            _typeActivator = null;
            _buildServiceProvider = buildServiceProvider;

            return this;
        }
        #endregion

        #region Exceptions
        /// <summary>
        /// Configures the application to use the specified implementation of <see cref="ICliExceptionHandler"/>.
        /// </summary>
        public CliApplicationBuilder UseExceptionHandler<T>()
            where T : class, ICliExceptionHandler, new()
        {
            _exceptionHandler = new T();
            return this;
        }

        /// <summary>
        /// Configures the application to use the specified implementation of <see cref="ICliExceptionHandler"/>.
        /// </summary>
        public CliApplicationBuilder UseExceptionHandler(ICliExceptionHandler handler)
        {
            _exceptionHandler = handler;
            return this;
        }
        #endregion

        #region Interactive Mode

        /// <summary>
        /// Specifies whether interactive mode (enabled with [interactive] directive) is allowed in the application.
        /// </summary>
        public CliApplicationBuilder AllowInteractiveMode(bool isAllowed = true)
        {
            _isInteractiveModeAllowed = isAllowed;
            return this;
        }
        #endregion

        /// <summary>
        /// Creates an instance of <see cref="CliApplication"/> using configured parameters.
        /// Default values are used in place of parameters that were not specified.
        /// </summary>
        public CliApplication Build()
        {
            _title ??= TryGetDefaultTitle() ?? "App";
            _executableName ??= TryGetDefaultExecutableName() ?? "app";
            _versionText ??= TryGetDefaultVersionText() ?? "v1.0";
            _console ??= new SystemConsole();
            _exceptionHandler ??= new DefaultExceptionHandler();

            var metadata = new ApplicationMetadata(_title, _executableName, _versionText, _description);
            var configuration = new ApplicationConfiguration(_commandTypes.ToArray(),
                                                             _customDirectives.ToArray(),
                                                             _exceptionHandler,
                                                             _isDebugModeAllowed,
                                                             _isPreviewModeAllowed,
                                                             _isInteractiveModeAllowed);

            CliContext cliContext = new CliContext(metadata, configuration, _console);

            if (_buildServiceProvider is null)
                _typeActivator ??= new DefaultTypeActivator();
            else
                _typeActivator = new DelegateTypeActivator(_buildServiceProvider.Invoke(cliContext, _console));

            if (_isInteractiveModeAllowed)
                return new InteractiveCliApplication(cliContext, _typeActivator);

            return new CliApplication(cliContext, _typeActivator);
        }
    }

    public partial class CliApplicationBuilder
    {
        private static readonly Lazy<Assembly?> LazyEntryAssembly = new Lazy<Assembly?>(Assembly.GetEntryAssembly);

        // Entry assembly is null in tests
        private static Assembly? EntryAssembly => LazyEntryAssembly.Value;

        private static string? TryGetDefaultTitle()
        {
            return EntryAssembly?.GetName().Name;
        }

        private static string? TryGetDefaultExecutableName()
        {
            string? entryAssemblyLocation = EntryAssembly?.Location;

            // The assembly can be an executable or a dll, depending on how it was packaged
            bool isDll = string.Equals(Path.GetExtension(entryAssemblyLocation), ".dll", StringComparison.OrdinalIgnoreCase);

            return isDll
                ? "dotnet " + Path.GetFileName(entryAssemblyLocation)
                : Path.GetFileNameWithoutExtension(entryAssemblyLocation);
        }

        private static string? TryGetDefaultVersionText()
        {
            return EntryAssembly != null ? $"v{EntryAssembly.GetName().Version.ToSemanticString()}" : null;
        }
    }
}