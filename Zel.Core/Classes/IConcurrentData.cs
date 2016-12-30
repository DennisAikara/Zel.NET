﻿// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Zel.Classes
{
    public interface IConcurrentData<T>
    {
        string UniqueIdentifier { get; }
        T Data { get; }
    }
}