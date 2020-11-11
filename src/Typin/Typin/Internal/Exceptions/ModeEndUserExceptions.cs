﻿namespace Typin.Internal.Exceptions
{
    using System;
    using System.Text;
    using Typin.Exceptions;
    using Typin.Schemas;

    /// <summary>
    /// End-user-facing exceptions. Avoid internal details and fix recommendations here
    /// </summary>
    internal static class ModeEndUserExceptions
    {
        public static TypinException InvalidStartupModeType(Type type)
        {
            var message = $"Cannot start the app. '{type.FullName}' is not a valid CLI mode type.";

            return new TypinException(message.Trim());
        }

        public static TypinException CommandExecutedInInvalidMode(CommandSchema command, Type currentMode)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"This application is running in '{currentMode}' mode.");
            builder.AppendLine($"However, command '{command.Type.FullName}' can be executed only from the following modes:");

            foreach (Type mode in command.SupportedModes!)
            {
                builder.AppendLine($"  - '{mode.FullName}'");
            }

            return new TypinException(builder.ToString());
        }

        public static TypinException DirectiveExecutedInInvalidMode(DirectiveSchema directive, Type currentMode)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"This application is running in '{currentMode}' mode.");
            builder.AppendLine($"However, directive '{directive.Type.FullName}' can be executed only from the following modes:");

            foreach (Type mode in directive.SupportedModes!)
            {
                builder.AppendLine($"  - '{mode.FullName}'");
            }

            return new TypinException(builder.ToString());
        }
    }
}