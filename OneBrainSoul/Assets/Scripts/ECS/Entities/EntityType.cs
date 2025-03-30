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
        SENDATU = 1 << 4,
        ENUM_SIZE = 1 << 5,
        ALL = ~0
    }
}