// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Zel.Classes
{
    public class LogData
    {
        public LogData(object data, string description = null)
        {
            Data = data;
            Description = description;

            if (data != null)
            {
                Type = data.GetType().FullName;
            }
        }

        public object Data { get; private set; }
        public string Description { get; private set; }
        public string Type { get; private set; }
    }
}