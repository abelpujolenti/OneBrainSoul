namespace AI.Combat.Enemy.LongArms
{
    public enum LongArmsAction : uint
    {
        OBSERVE,
        GO_TO_CLOSEST_SIGHTED_TARGET,
        ACQUIRE_NEW_TARGET_FOR_THROW_ROCK,
        ACQUIRE_NEW_TARGET_FOR_CLAP_ABOVE,
        THROW_ROCK,
        CLAP_ABOVE,
        FLEE,
        ENUM_SIZE
    }
}