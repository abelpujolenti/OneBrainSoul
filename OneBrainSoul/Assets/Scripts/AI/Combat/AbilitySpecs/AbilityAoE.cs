using System;
using UnityEngine;

namespace AI.Combat.AbilitySpecs
{
    public enum AbilityAoEType
    {
        RECTANGULAR,
        SPHERICAL,
        CONICAL,
        CUSTOM_MESH
    }
    
    [Serializable]
    public class AbilityAoE
    {
        public bool isAttachedToCaster;
        
        public Vector3 relativePositionToCaster;
        
        public float duration;
        
        public Vector3 direction;

        public float height;
        public float width;
        public float length;
        
        public float radius;

        public GameObject customMeshPrefab;
        public float scale;

        public bool doesHeightChangeOverTheTime;
        public bool doesWidthChangeOverTheTime;
        public bool doesLengthChangeOverTheTime;
        public bool doesRadiusChangeOverTheTime;
        public bool doesScaleChangeOverTheTime;
        public bool doesXScaleChangeOverTheTime;
        public bool doesYScaleChangeOverTheTime;
        public bool doesZScaleChangeOverTheTime;

        public AnimationCurve heightChangeOverTime = new AnimationCurve();
        public AnimationCurve widthChangeOverTime = new AnimationCurve();
        public AnimationCurve lengthChangeOverTime = new AnimationCurve();
        public AnimationCurve radiusChangeOverTime = new AnimationCurve();
        public AnimationCurve scaleChangeOverTime = new AnimationCurve();
        public AnimationCurve XScaleChangeOverTime = new AnimationCurve();
        public AnimationCurve YScaleChangeOverTime = new AnimationCurve();
        public AnimationCurve ZScaleChangeOverTime = new AnimationCurve();

        public GameObject objectWithParticleSystem;
        public Vector3 relativePositionForParticles;
    }
}