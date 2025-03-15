using Interfaces.AI.Navigation;
using UnityEngine;

namespace AI.Navigation
{
    public class DynamicObstacleThreadSafe
    {
        private Vector3 position;
        private float radius;

        public DynamicObstacleThreadSafe(Vector3 position, float radius)
        {
            this.position = position;
            this.radius = radius;
        }

        public void SetPositionAndRadius(Vector3 position, float radius)
        {
            this.position = position;
            this.radius = radius;
        }

        public Vector3 GetPosition()
        {
            return position;
        }

        public float GetRadius()
        {
            return radius;
        }
    }
}