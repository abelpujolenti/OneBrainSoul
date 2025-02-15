using System;

namespace ECS.Entities
{
    [Flags]
    public enum EntityType
    {
        NONE = 0,
        PLAYER = 1 << 0,
        TRIFACE = 1 << 1,
        LONG_ARMS = 1 << 2,
        LONG_ARMS_BASE = 1 << 3,
        SENDATU = 1 << 4
    }
}