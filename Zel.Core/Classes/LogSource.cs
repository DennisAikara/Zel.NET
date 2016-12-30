// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel.Classes
{
    public class LogSource
    {
        public LogSource(string source, string sourceId)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (sourceId == null)
            {
                throw new ArgumentNullException("sourceId");
            }

            source = source.Trim();
            sourceId = sourceId.Trim();

            if ((source.Length == 0) || (source.Length > 50) || (sourceId.Length == 0) || (sourceId.Length > 32))
            {
                throw new Exception("Invalid LogSource");
            }

            Source = source;
            SourceId = sourceId;
        }

        public string Source { get; set; }
        public string SourceId { get; set; }
    }
}