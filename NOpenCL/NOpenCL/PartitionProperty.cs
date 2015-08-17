/*
 * Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 * Modified for use in Asm4GCN. See https://github.com/tunnelvisionlabs/NOpenCL for original.*/

namespace NOpenCL
{
    public enum PartitionProperty
    {
        PartitionEqually = 0x1086,
        PartitionByCounts = 0x1087,
        PartitionByCountsListEnd = 0x0000,
        PartitionByAffinityDomain = 0x1088,
    }
}
