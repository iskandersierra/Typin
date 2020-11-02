﻿namespace TypinExamples.Configuration
{
    using System;

    public sealed class HeaderSettings
    {
        /// <summary>
        /// Heading with Markdown formatting.
        /// </summary>
        public string? Heading { get; set; }

        /// <summary>
        /// Subheading with markdown formatting.
        /// </summary>
        public string? Subheading { get; set; }

        /// <summary>
        /// Links colleciton.
        /// </summary>
        public LinkDefinition[] Links { get; set; } = Array.Empty<LinkDefinition>();
    }
}
