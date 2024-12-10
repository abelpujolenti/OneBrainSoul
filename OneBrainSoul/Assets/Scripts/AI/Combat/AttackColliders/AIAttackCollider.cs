using System;
using Managers;
using UnityEngine;

namespace AI.Combat.AttackColliders
{
    public abstract class AIAttackCollider : MonoBehaviour
    {
        public abstract void SetAttackTargets(int targetsLayerMask);
        public abstract void StartInflictingDamage();

        protected Quaternion _parentRotation;

        protected abstract void OnEnable();
        protected abstract void OnDisable();

        private void Start()
        {
            EventsManager.OnAgentDefeated += RemoveAgentID;
        }

        private void OnDestroy()
        {
            EventsManager.OnAgentDefeated -= RemoveAgentID;
        }

        protected abstract void RemoveAgentID(uint agentID);

        public void SetParent(Transform parentTransform)
        {
            transform.parent = parentTransform;
            _parentRotation = parentTransform.rotation;
        }

        protected void MoveToPosition(Vector3 position)
        {
            gameObject.transform.localPosition = position;
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }
    }
}