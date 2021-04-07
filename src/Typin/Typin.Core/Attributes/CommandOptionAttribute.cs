﻿namespace Typin.Attributes
{
    using System;
    using Typin.Binding;
    using Typin.Input;
    using Typin.Internal.Exceptions;
    using Typin.Internal.Extensions;

    /// <summary>
    /// Annotates a property that defines a command option.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CommandOptionAttribute : Attribute
    {
        /// <summary>
        /// Option name (must be longer than a single character). Starting dashes are trimed automatically.
        /// All options in a command must have different names (comparison is case-sensitive).
        /// If this isn't specified, kebab-cased property name is used instead.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Option short name (single character).
        /// All options in a command must have different short names (comparison is case-sensitive).
        /// </summary>
        public char? ShortName { get; }

        /// <summary>
        /// Whether an option is required.
        /// </summary>
        public bool IsRequired { get; init; }

        /// <summary>
        /// Option description, which is used in help text.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Fallback variable that will be used as fallback if no option value is specified.
        /// </summary>
        public string? FallbackVariableName { get; init; }

        /// <summary>
        /// Whether option has auto generated name.
        /// </summary>
        public bool HasAutoGeneratedName => Name is null && ShortName is null;

        /// <summary>
        /// Binding converter.
        /// </summary>
        public Type? Converter { get; init; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionAttribute"/>.
        /// </summary>
        private CommandOptionAttribute(string? name, char? shortName)
        {
            // The user may mistakenly specify dashes, thinking it's required, so trim them
            Name = name?.TrimStart('-');
            ShortName = shortName;

            if (!(Name is null && ShortName is null))
            {
                if (Name is string n && (n.Contains(' ') || !CommandOptionInput.IsOption("--" + n)))
                    throw AttributesExceptions.InvalidOptionName(n);

                if (shortName is char sn && !CommandOptionInput.IsOptionAlias("-" + sn))
                    throw AttributesExceptions.InvalidOptionShortName(sn);
            }

            if (Converter is Type t && !t.Implements(typeof(IBindingConverter)))
            {
                throw AttributesExceptions.InvalidConverterType(t);
            }
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionAttribute"/>.
        /// </summary>
        public CommandOptionAttribute(string name, char shortName)
            : this(name, (char?)shortName)
        {

        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionAttribute"/>.
        /// </summary>
        public CommandOptionAttribute(string name)
            : this(name, null)
        {

        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionAttribute"/>.
        /// </summary>
        public CommandOptionAttribute()
            : this(null, null)
        {

        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionAttribute"/>.
        /// </summary>
        public CommandOptionAttribute(char shortName)
            : this(null, (char?)shortName)
        {

        }
    }
}