/*
 * Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 * Modified for use in Asm4GCN. See https://github.com/tunnelvisionlabs/NOpenCL for original.*/

namespace NOpenCL
{
    using System;

    [Flags]
    public enum MapFlags : ulong
    {
        None = 0,
        Read = 1 << 0,
        Write = 1 << 1,
        WriteInvalidateRegion = 1 << 2,
    }
}
