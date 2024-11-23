using System.Collections.Generic;

namespace ECS.Components.AI.Combat
{
    public class ThreatGroupComponent : GroupComponent
    {
        public float groupRadius;

        public Dictionary<uint, SubThreatGroupComponent> subThreatGroups =
            new Dictionary<uint, SubThreatGroupComponent>();

        public ThreatGroupComponent(float threatGroupWeight)
        {
            groupWeight = threatGroupWeight;
        }
    }
}