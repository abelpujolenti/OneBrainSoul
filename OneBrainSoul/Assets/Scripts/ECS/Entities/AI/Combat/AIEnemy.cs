using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using AI.Combat;
using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Combat;
using Managers;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public class AIEnemy : AICombatAgentEntity<AIEnemyContext, AttackComponent, AllyDamageComponent>
    {
        [SerializeField] private AIEnemySpecs _aiEnemySpecs;
        
        private void Start()
        {
            Setup();
            InstantiateAttackComponents(_aiEnemySpecs.aiAttacks);
            CalculateMinimumAndMaximumRangeToAttacks(_attackComponents);

            _raysTargetsLayerMask = (int)(Math.Pow(2, GameManager.Instance.GetEnemyLayer()) + 
                                          Math.Pow(2, GameManager.Instance.GetGroundLayer()));
            
            _context = new AIEnemyContext(_aiEnemySpecs.totalHealth, GetComponent<CapsuleCollider>().radius, 
                _aiEnemySpecs.sightMaximumDistance, _minimumRangeToCastAnAttack, _maximumRangeToCastAnAttack, transform, 
                _aiEnemySpecs.maximumStress, _aiEnemySpecs.stunDuration);
            
            CombatManager.Instance.AddAIEnemy(this);
            
            StartUpdate();
        }

        private void InstantiateAttackComponents(List<AIAttack> attacks)
        {
            foreach (AIAttack aiAttack in attacks)
            {
                switch (aiAttack.aiAttackAoEType)
                {
                    case AIAttackAoEType.RECTANGLE_AREA:
                        _attackComponents.Add(new RectangleAttackComponent(aiAttack));
                        break;
                    
                    case AIAttackAoEType.CIRCLE_AREA:
                        _attackComponents.Add(new CircleAttackComponent(aiAttack));
                        break;
                    
                    case AIAttackAoEType.CONE_AREA:
                        _attackComponents.Add(new ConeAttackComponent(aiAttack));
                        break;
                }
            }
        }

        protected override IEnumerator UpdateCoroutine()
        {
            while (true)
            {
                UpdateVisibleRivals();

                UpdateVectorToRival();

                if (_context.IsAttacking())
                {
                    yield return null;
                    continue;
                }
                
                SetRaysDirections();
            
                CalculateBestAction();

                yield return null;
            }
        }

        protected override void UpdateVisibleRivals()
        {
            _visibleRivals = CombatManager.Instance.GetVisibleRivals
                <AIAlly, AIAllyContext, AllyAttackComponent, DamageComponent, 
                    AIEnemyContext, AttackComponent, AllyDamageComponent>(this);

            _context.SetIsSeeingARival(_visibleRivals.Count != 0);
        }

        protected override void CalculateBestAction()
        {
            CombatManager.Instance.CalculateBestAction(this);
        }

        public AttackComponent Attack()
        {
            StopNavigation();

            AttackComponent attackComponent = ReturnNextAttack();
            
            Attacking();

            return attackComponent;
        }

        public override void OnReceiveDamage(AllyDamageComponent damageComponent)
        {
            _context.SetHealth(_context.GetHealth() - damageComponent.GetDamage());

            uint health = _context.GetHealth();

            if (health == 0)
            {
                OnDefeated();
                return;
            }

            uint combatAgentInstanceID = GetAgentID();
            
            bool isStunned = _context.IsStunned();

            if (!isStunned)
            {
                _context.SetCurrentStress(_context.GetCurrentStress() + damageComponent.GetStressDamage());
                isStunned = _context.IsStunned();
                
                if (isStunned)
                {
                    StartCoroutine(StunDuration());
                }
                else
                {
                    //TODO FEEDBACK
                }
                
                CombatManager.Instance.OnEnemyReceiveDamage(combatAgentInstanceID, health, _context.GetCurrentStress(), isStunned);
                return;
            }
            
            CombatManager.Instance.OnEnemyReceiveDamage(combatAgentInstanceID, health, _context.GetCurrentStress(), true);
        }

        protected override void OnDefeated()
        {
            CombatManager.Instance.OnEnemyDefeated(this);
            ECSNavigationManager.Instance.RemoveNavMeshAgentEntity(GetAgentID());
            Destroy(gameObject);
        }

        private IEnumerator StunDuration()
        {
            float stunDuration = GetContext().GetStunDuration();
            float time = 0;

            GetNavMeshAgentComponent().GetNavMeshAgent().isStopped = true;
            
            while (time < stunDuration)
            {
                time += Time.deltaTime;
                yield return null;
            }

            GetNavMeshAgentComponent().GetNavMeshAgent().isStopped = false;
            
            GetContext().SetIsStunned(false);
            
            RotateToNextPathCorner();
            CombatManager.Instance.OnEnemyStunEnds(GetAgentID());
        }

        public override AIAgentType GetAIAgentType()
        {
            return _aiEnemySpecs.aiAgentType;
        }

        public override AIEnemyContext GetContext()
        {
            return _context;
        }

        public void SetCurrentStress(float currentStress)
        {
            _context.SetCurrentStress(currentStress);
        }

        public List<AttackComponent> GetAttackComponents()
        {
            return _attackComponents;
        }

        private void OnDisable()
        {
            OnDefeated();
        }
    }
}
