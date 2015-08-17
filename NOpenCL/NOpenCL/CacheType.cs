﻿/*
 * Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 * Modified for use in Asm4GCN. See https://github.com/tunnelvisionlabs/NOpenCL for original.*/

namespace NOpenCL
{
    /// <summary>
    /// Specifies the type of global memory cache supported.
    /// </summary>
    public enum CacheType
    {
        None = 0,
        ReadOnly = 1,
        ReadWrite = 2,
    }
}
