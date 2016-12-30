// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Zel.Classes
{
    public abstract class LogStore : ILogStore
    {
        #region ILogStore Members

        public abstract bool WriteToLog(LogMessage message);

        #endregion

        protected string GetHostCode()
        {
            var hostCode = Asp.GetHostCode();
            return hostCode == 0 ? "000000" : hostCode.ToString();
        }
    }
}