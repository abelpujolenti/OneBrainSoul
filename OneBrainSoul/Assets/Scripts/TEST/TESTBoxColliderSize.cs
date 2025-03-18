using UnityEngine;

namespace TEST
{
    public class TESTBoxColliderSize : MonoBehaviour
    {
        [SerializeField] private BoxCollider _boxCollider;

        [SerializeField] private Transform ownTransform;

        private void Update()
        {
            ownTransform.localScale = _boxCollider.size;
        }
    }
}