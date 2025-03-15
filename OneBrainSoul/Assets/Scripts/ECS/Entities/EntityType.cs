using System;

namespace ECS.Entities
{
    [Flags]
    public enum EntityType
    {
        NONE = 0,
        PLAYER = 1 << 0,
        GHOST = 1 << 1,
        TRIFACE = 1 << 2,
        LONG_ARMS = 1 << 3,
        LONG_ARMS_BASE = 1 << 4,
        SENDATU = 1 << 5,
        ENUM_SIZE = 1 << 6,
        ALL = ~0
    }
}