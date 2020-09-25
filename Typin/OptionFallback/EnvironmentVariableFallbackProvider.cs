﻿namespace Typin.OptionFallback
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Environment variable fallback value provider.
    /// </summary>
    public class EnvironmentVariableFallbackProvider : IOptionFallbackProvider
    {
        private readonly IReadOnlyDictionary<string, string> _environmentVariables;

        /// <summary>
        /// Initializes an instance of <see cref="EnvironmentVariableFallbackProvider"/>.
        /// </summary>
        public EnvironmentVariableFallbackProvider(IReadOnlyDictionary<string, string> environmentVariables)
        {
            _environmentVariables = environmentVariables;
        }

        /// <inheritdoc/>
        public IEnumerable<string> Keys => _environmentVariables.Keys;

        /// <inheritdoc/>
        public IEnumerable<string> Values => _environmentVariables.Values;

        /// <inheritdoc/>
        public int Count => _environmentVariables.Count;

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            return _environmentVariables.ContainsKey(key);
        }

        /// <inheritdoc/>
        public string this[string key]
        {
            get => _environmentVariables[key];
        }

        /// <inheritdoc/>
        public bool TryGetValue(string key, out string value)
        {
            return _environmentVariables.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _environmentVariables.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (_environmentVariables as IEnumerable).GetEnumerator();
        }
    }
}
