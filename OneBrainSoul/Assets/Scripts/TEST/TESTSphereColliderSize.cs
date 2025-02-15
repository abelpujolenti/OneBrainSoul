using UnityEngine;

namespace TEST
{
    public class TESTSphereColliderSize : MonoBehaviour
    {
        [SerializeField] private SphereCollider _sphereCollider;

        [SerializeField] private Transform ownTransform;

        private void Update()
        {
            ownTransform.localScale = new Vector3(_sphereCollider.radius * 2, _sphereCollider.radius * 2, _sphereCollider.radius * 2);
        }
    }
}