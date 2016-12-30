// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Zel.DataAccess
{
    [AttributeUsage(AttributeTargets.Assembly)]
    [Serializable]
    public class DataAccessAssemblyAttribute : Attribute {}
}