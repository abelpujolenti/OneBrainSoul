using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace AI.Combat.Position
{
    public class SurroundingSlots
    {
        private readonly float _radiusFromAgent;
        private Dictionary<uint, AgentSlot> _rivalSlots = new Dictionary<uint, AgentSlot>();

        public SurroundingSlots(float radiusFromAgent)
        {
            _radiusFromAgent = radiusFromAgent;
        }

        public AgentSlotPosition ReserveSubtendedAngle(uint agentID, Vector3 vectorFromTarget, float radius)
        {
            if (_rivalSlots.ContainsKey(agentID) && vectorFromTarget.magnitude - radius < _radiusFromAgent)
            {
                return null;
            }
            
            float angle = MathUtil.VectorXZToYAxisAngle(-vectorFromTarget);
            float subtendedAngle = CalculateSubtendedAngle(radius) + 10;

            PriorityQueue<float> angles = new PriorityQueue<float>();
            
            angles.Enqueue(angle, 0);

            float halfSubtendedAngle = subtendedAngle / 2;

            float minimumAngleAllowed;
            float maximumAngleAllowed;

            (bool, float) doesExistAvailableAngle;

            bool containsKey = _rivalSlots.ContainsKey(agentID);
            
            if (containsKey)
            {
                float previousAngle = _rivalSlots[agentID].angle;
                
                minimumAngleAllowed = angle - previousAngle - halfSubtendedAngle;
                maximumAngleAllowed = angle + previousAngle + halfSubtendedAngle;
                
                doesExistAvailableAngle = ModifyAvailableAngle(angle, previousAngle, angles, subtendedAngle, 
                    halfSubtendedAngle, minimumAngleAllowed, maximumAngleAllowed);
            }
            else
            {
                minimumAngleAllowed = angle - 180 - halfSubtendedAngle;
                maximumAngleAllowed = angle + 180 + halfSubtendedAngle;

                doesExistAvailableAngle = ReturnAvailableAngle(angle, angles, subtendedAngle, 
                    halfSubtendedAngle, minimumAngleAllowed, maximumAngleAllowed);
            }

            if (!doesExistAvailableAngle.Item1)
            {
                return null;
            }

            if (containsKey)
            {
                _rivalSlots[agentID].angle = doesExistAvailableAngle.Item2;
            }
            else
            {
                AgentSlot newAgentSlot = new AgentSlot
                {
                    angle = doesExistAvailableAngle.Item2,
                    subtendedAngle = subtendedAngle
                };
            
                _rivalSlots.Add(agentID, newAgentSlot);
            }

            return new AgentSlotPosition
            {
                deviationVector = CalculateDeviationVector(_rivalSlots[agentID].angle, radius),
                agentSlot = _rivalSlots[agentID]
            };
        }

        private (bool, float) ModifyAvailableAngle(float newAngle, float oldAngle, PriorityQueue<float> angles,
            float subtendedAngle, float halfSubtendedAngle, float minimumAngleAllowed, float maximumAngleAllowed)
        {
            float angle = angles.Dequeue();

            float minimumAngle = angle - halfSubtendedAngle;
            float maximumAngle = angle + halfSubtendedAngle;

            if (!CheckIfAngleIsBetweenRange(minimumAngle, minimumAngleAllowed, maximumAngleAllowed, false) || 
                !CheckIfAngleIsBetweenRange(maximumAngle, minimumAngleAllowed, maximumAngleAllowed, false))
            {
                return (false, angle);
            }

            if (minimumAngle < 0f)
            {
                minimumAngle += 360;
            }

            maximumAngle %= 360;
            
            bool wraps0;
            
            foreach (AgentSlot rivalSlot in _rivalSlots.Values)
            {
                if (Math.Abs(rivalSlot.angle - oldAngle) < 0.1f)
                {
                    continue;
                }
                
                float currentMinimumAngle = rivalSlot.angle - rivalSlot.subtendedAngle / 2;

                if (currentMinimumAngle < 0f)
                {
                    currentMinimumAngle += 360;
                }
                
                float currentMaximumAngle = (rivalSlot.angle + rivalSlot.subtendedAngle / 2) % 360;
                
                wraps0 = currentMaximumAngle < currentMinimumAngle;

                if (!CheckIfAngleIsBetweenRange(angle, currentMinimumAngle, currentMaximumAngle, wraps0) &&
                    !CheckIfAngleIsBetweenRange(minimumAngle, currentMinimumAngle, currentMaximumAngle, wraps0) &&
                    !CheckIfAngleIsBetweenRange(maximumAngle, currentMinimumAngle, currentMaximumAngle, wraps0))
                {
                    continue;
                }

                float newSideAngle = currentMaximumAngle + subtendedAngle / 2;
                angles.Enqueue(newSideAngle, Mathf.Abs(newAngle - newSideAngle));

                newSideAngle = currentMinimumAngle - subtendedAngle / 2;
                angles.Enqueue(newSideAngle, Mathf.Abs(newAngle - newSideAngle));

                return ModifyAvailableAngle(newAngle, oldAngle, angles, subtendedAngle, halfSubtendedAngle, 
                    minimumAngleAllowed, maximumAngleAllowed);
            }

            return (true, angle);
        }

        private (bool, float) ReturnAvailableAngle(float originalAngle, PriorityQueue<float> angles, 
            float subtendedAngle, float halfSubtendedAngle, float minimumAngleAllowed, float maximumAngleAllowed)
        {
            float angle = angles.Dequeue();

            float minimumAngle = angle - halfSubtendedAngle;
            float maximumAngle = angle + halfSubtendedAngle;

            if (!CheckIfAngleIsBetweenRange(minimumAngle, minimumAngleAllowed, maximumAngleAllowed, false) || 
                !CheckIfAngleIsBetweenRange(maximumAngle, minimumAngleAllowed, maximumAngleAllowed, false))
            {
                return (false, angle);
            }

            if (minimumAngle < 0f)
            {
                minimumAngle += 360;
            }

            maximumAngle %= 360;
            
            bool wraps0;
            
            foreach (AgentSlot rivalSlot in _rivalSlots.Values)
            {
                float currentMinimumAngle = rivalSlot.angle - rivalSlot.subtendedAngle / 2;

                if (currentMinimumAngle < 0f)
                {
                    currentMinimumAngle += 360;
                }
                
                float currentMaximumAngle = (rivalSlot.angle + rivalSlot.subtendedAngle / 2) % 360;

                wraps0 = currentMaximumAngle < currentMinimumAngle;

                if (!CheckIfAngleIsBetweenRange(angle, currentMinimumAngle, currentMaximumAngle, wraps0) &&
                    !CheckIfAngleIsBetweenRange(minimumAngle, currentMinimumAngle, currentMaximumAngle, wraps0) &&
                    !CheckIfAngleIsBetweenRange(maximumAngle, currentMinimumAngle, currentMaximumAngle, wraps0))
                {
                    continue;
                }

                float newSideAngle = currentMaximumAngle + subtendedAngle / 2;
                angles.Enqueue(newSideAngle, Mathf.Abs(originalAngle - newSideAngle));

                newSideAngle = currentMinimumAngle - subtendedAngle / 2;
                angles.Enqueue(newSideAngle, Mathf.Abs(originalAngle - newSideAngle));

                return ReturnAvailableAngle(originalAngle, angles, subtendedAngle, halfSubtendedAngle, 
                    minimumAngleAllowed, maximumAngleAllowed);
            }

            return (true, angle);
        }

        private Vector3 CalculateDeviationVector(float angle, float radius)
        {
            float radians = MathUtil.AngleToRadians(angle);

            Vector3 deviationVector = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)).normalized;

            return deviationVector * (_radiusFromAgent + radius);
        }

        public void FreeSubtendedAngle(uint agentID)
        {
            _rivalSlots.Remove(agentID);
        }

        private float CalculateSubtendedAngle(float radius)
        {
            float radians = 2 * Mathf.Asin(radius / _radiusFromAgent);

            return MathUtil.RadiansToAngle(radians);
        }

        private bool CheckIfAngleIsBetweenRange(float angleToCheck, float minimumAngeRange, float maximumAngleRange,
            bool wraps0)
        {
            return wraps0 
                ? angleToCheck > minimumAngeRange ^ angleToCheck < maximumAngleRange
                : angleToCheck > minimumAngeRange && angleToCheck < maximumAngleRange;
        }
    }
}