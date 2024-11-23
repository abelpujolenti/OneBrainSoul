﻿using System;
using Interfaces.AI.Combat;

namespace ECS.Components.AI.Combat
{
    [Serializable]
    public class ThreatComponent : IStatWeight, IGroup
    {
        private static uint _globalThreatIndex = 1;
        
        private uint _originalThreatGroup;
        public uint currentGroup;
        
        private float _threatWeight;

        public ThreatComponent(float threatWeight)
        {
            _originalThreatGroup = _globalThreatIndex;
            currentGroup = _originalThreatGroup;
            _threatWeight = threatWeight;
            _globalThreatIndex++;
        }

        public float GetWeight()
        {
            return _threatWeight;
        }

        public uint GetOriginalGroup()
        {
            return _originalThreatGroup;
        }

        public uint GetCurrentGroup()
        {
            return currentGroup;
        }
    }
}