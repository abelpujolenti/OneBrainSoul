namespace AI.Combat.Enemy.Triface
{
    public enum TrifaceAction : uint
    {
        CONTINUE_NAVIGATION,
        ROTATE,
        PATROL,
        INVESTIGATE_AREA,
        GO_TO_CLOSEST_SIGHTED_TARGET,
        ACQUIRE_NEW_TARGET_FOR_SLAM,
        SLAM,
        ENUM_SIZE
    }
}