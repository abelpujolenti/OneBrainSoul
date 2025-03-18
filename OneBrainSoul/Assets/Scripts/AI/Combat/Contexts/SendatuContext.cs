using System.Collections.Generic;
using AI.Combat.Enemy.LongArms;
using ECS.Entities;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public class SendatuContext : TeleportMobilityEnemyContext
    {
        private Dictionary<uint, float> _distancesToTargetsToFleeFrom;
        private float _radiusToFlee;
        
        public SendatuContext(uint totalHealth,float radius, float height,
            Transform headAgentTransform, Transform bodyAgentTransform, float radiusToFlee) : 
            base(EntityType.SENDATU, totalHealth, radius, height, headAgentTransform, bodyAgentTransform)
        {
            _repeatableActions = new List<uint>
            {
                (uint)LongArmsAction.OBSERVE,
                (uint)LongArmsAction.THROW_ROCK
            };

            _distancesToTargetsToFleeFrom = new Dictionary<uint, float>();
            _radiusToFlee = radiusToFlee;
        }

        public float GetDistanceToClosestTargetToFleeFrom()
        {
            float minimumDistance = Mathf.Infinity;

            foreach (float distance in _distancesToTargetsToFleeFrom.Values)
            {
                if (minimumDistance < distance)
                {
                    continue;
                }

                minimumDistance = distance;
            }
            
            return minimumDistance;
        }

        public void SetDistanceToTargetToFleeFrom(uint agentID, float distance)
        {
            if (_distancesToTargetsToFleeFrom.ContainsKey(agentID))
            {
                _distancesToTargetsToFleeFrom[agentID] = distance;
                return;
            }
            
            _distancesToTargetsToFleeFrom.Add(agentID, distance);
        }

        public float GetRadiusToFlee()
        {
            return _radiusToFlee;
        }

        public override bool IsSeeingATarget()
        {
            return false;
        }

        public override bool HasATarget()
        {
            return false;
        }
    }
}