using ECS.Components.AI.Navigation;
using Interfaces.AI.Navigation;
using Managers;
using UnityEngine;

namespace Player
{
    public class PlayerStatus : MonoBehaviour, IPosition
    {
        private IPosition _position;

        private float _height;
        private float _radius;

        private void Start()
        {
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

            _height = capsuleCollider.height;
            _radius = capsuleCollider.radius;
            
            _position = new TransformComponent(transform);
            CombatManager.Instance.AddPlayer(this);
        }

        public Vector3 GetPosition()
        {
            return _position.GetPosition();
        }

        public float GetHeight()
        {
            return _height;
        }

        public float GetRadius()
        {
            return _radius;
        }
    }
}