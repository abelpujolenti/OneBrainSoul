using ECS.Entities.AI;
using UnityEngine;

namespace Combat
{
    public class WandProjectile : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Material _material;
        float speed;
        bool empowered;
        float lifeTime;
        float landTime;
        float deathTime;
        float deathDuration = .075f;
        float landDuration = .1f;
        float landExplosionScaleAdd = 8f;
        private float _originalScale;

        private void Start()
        {
            gameObject.SetActive(false);
            _originalScale = transform.localScale.x;
        }

        public void Init(float lifeTime, float speed)
        {
            this.lifeTime = lifeTime;
            this.speed = speed;
        }

        private void OnEnable()
        {
            ResetValues();
        }

        private void ResetValues()
        {
            lifeTime = 0f;
            landTime = 0f;
            deathTime = 0f;
            deathDuration = .075f;
            landDuration = .1f;
            landExplosionScaleAdd = 8f;
        }

        public void Shoot(bool isPlayerOnTheGround, Vector3 direction)
        {
            empowered = !isPlayerOnTheGround;
        
            if (empowered)
            {
                _material.SetColor("_Color", new Color(0f, .7f, 1f));
            }
            _rigidbody.AddForce(direction * speed);
        }
    
        private void OnCollisionEnter(Collision collision)
        {
            if (deathTime > 0f || landTime > 0f) return;
            AgentEntity entity = collision.collider.GetComponent<AgentEntity>();
            if (entity != null)
            {
                Land(entity);
            }
            else
            {
                deathTime = deathDuration;
            }
        }

        private void Update()
        {
            if (lifeTime <= 0f && deathTime == 0f)
            {
                deathTime = deathDuration;
            }

            if (landTime > 0f)
            {
                float s = _originalScale + Mathf.Pow(1f - landTime / landDuration, 2f) * landExplosionScaleAdd;
                transform.localScale = new Vector3(s, s, s);
                float o = Mathf.Pow(landTime / landDuration, 2f);
                _material.SetFloat("_Opacity", o);
                if (landTime - Time.deltaTime <= 0f)
                {
                    gameObject.SetActive(false);
                }
            
            }
            else if (deathTime > 0f)
            {
                float s = Mathf.Pow(deathTime / deathDuration, 2f);
                _material.SetFloat("_Progress", s);
                if (deathTime - Time.deltaTime <= 0f)
                {
                    gameObject.SetActive(false);
                }
            }

            landTime = Mathf.Max(0f, landTime - Time.deltaTime);
            deathTime = Mathf.Max(0f, deathTime - Time.deltaTime);
            lifeTime -= Time.deltaTime;
        }

        private void Land(AgentEntity entity)
        {
            landTime = landDuration;
            entity.OnReceiveDamage((uint)(empowered ? 3 : 2), transform.position, transform.position);
        }
    }
}
