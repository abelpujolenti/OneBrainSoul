namespace ECS.Components.AI.Combat
{
    public class MoralGroupComponent : GroupComponent
    {
        public float helpPriority;

        public float threatSuffering;
        
        public MoralGroupComponent(float moralGroupWeight)
        {
            groupWeight = moralGroupWeight;
        }
    }
}