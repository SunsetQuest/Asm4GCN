﻿/*
 * Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 * Modified for use in Asm4GCN. See https://github.com/tunnelvisionlabs/NOpenCL for original.*/

namespace NOpenCL
{
    public enum CommandType
    {
        NdrangeKernel = 0x11F0,
        Task = 0x11F1,
        NativeKernel = 0x11F2,
        ReadBuffer = 0x11F3,
        WriteBuffer = 0x11F4,
        CopyBuffer = 0x11F5,
        ReadImage = 0x11F6,
        WriteImage = 0x11F7,
        CopyImage = 0x11F8,
        CopyImageToBuffer = 0x11F9,
        CopyBufferToImage = 0x11FA,
        MapBuffer = 0x11FB,
        MapImage = 0x11FC,
        UnmapMemObject = 0x11FD,
        Marker = 0x11FE,
        AcquireGlObjects = 0x11FF,
        ReleaseGlObjects = 0x1200,
        ReadBufferRect = 0x1201,
        WriteBufferRect = 0x1202,
        CopyBufferRect = 0x1203,
        User = 0x1204,
        Barrier = 0x1205,
        MigrateMemObjects = 0x1206,
        FillBuffer = 0x1207,
        FillImage = 0x1208,
    }
}
