using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class AIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterBase m_character = null;

        [Header("Target Following Settings")]
        [SerializeField][Min(1.0f)] private float m_detectionRadius = 5.0f;
        [SerializeField][Min(1.0f)] private float m_resetFromInitialPositionRadius = 10.0f;
        [SerializeField][Min(1.0f)] private float m_resetFromTargetDistanceRadius = 10.0f;
        [SerializeField][Min(0.5f)] private float m_retargetCooldown = 3.0f;
        [SerializeField][Min(0.1f)] private float m_soughtDistanceFromTarget = 1.0f;

        [Header("Attack Settings")]
        [SerializeField] public float m_attackTriggerRadius = 1.0f;
        [SerializeField] public float m_attackCooldown = 1.0f;

        private Vector2 m_initialPosition;
        private Transform m_target = null;
        private float m_retargetCooldownTimer = 0.0f;
        private float m_attackCooldownTimer = 0.0f;

        private void Awake()
        {
            m_initialPosition = transform.position;
        }

        private Transform FindTarget()
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, m_detectionRadius, Vector2.zero, 0.0f);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform.TryGetComponent(out CharacterBase character) && character.CanBeAttackedBy(m_character))
                {
                    return hit.transform;
                }
            }

            return null;
        }

        private Vector2 GetTargetPosition()
        {
            return m_target ? (Vector2)m_target.position : m_initialPosition;
        }

        private Vector2 GetTargetMovementDirection()
        {
            Vector2 targetPosition = GetTargetPosition();
            Vector2 currentPosition = transform.position;
            Vector2 targetMovementDirection = targetPosition - currentPosition;

            if (targetMovementDirection.magnitude > m_soughtDistanceFromTarget)
            {
                targetMovementDirection.Normalize();
                return targetMovementDirection;
            }

            return Vector2.zero;
        }

        private void UpdateCooldowns()
        {
            if (m_retargetCooldownTimer > 0.0f)
            {
                m_retargetCooldownTimer = Math.Max(m_retargetCooldownTimer - Time.fixedDeltaTime, 0.0f);
            }

            if (m_attackCooldownTimer > 0.0f)
            {
                m_attackCooldownTimer = Math.Max(m_attackCooldownTimer - Time.fixedDeltaTime, 0.0f);
            }
        }

        private void TryToAttackTarget(float distanceToTarget)
        {
            if (m_attackCooldownTimer == 0.0f && distanceToTarget < m_attackTriggerRadius)
            {
                // Find the first triggerable ability available on the character and fire it
                foreach (AbilityBase ability in m_character.abilityInstances)
                {
                    if (ability is ITriggerableAbility)
                    {
                        m_character.FireAbility((ITriggerableAbility)ability);
                        m_attackCooldownTimer = m_attackCooldown;
                        break;
                    }
                }
            }
        }

        private void CheckIfTargetOutOfRange(float distanceToTarget)
        {
            float distanceToInitialPosition = Vector2.Distance(m_initialPosition, transform.position);
            bool isTooFarFromInitialPosition = distanceToInitialPosition > m_resetFromInitialPositionRadius;
            bool isTooFarFromTarget = distanceToTarget > m_resetFromTargetDistanceRadius;

            if (isTooFarFromInitialPosition || isTooFarFromTarget)
            {
                m_retargetCooldownTimer = m_retargetCooldown;
                m_target = null;
            }
        }

        private void FixedUpdate()
        {
            UpdateCooldowns();

            if (!m_target)
            {
                if (m_retargetCooldownTimer == 0.0f)
                {
                    m_target = FindTarget();
                    if (m_target)
                    {
                        GameManager.NotificationSystem.targetDetected.Invoke(this, m_target);
                    }
                }
            }
            else
            {
                float distanceToTarget = Vector2.Distance(m_target.position, transform.position);

                TryToAttackTarget(distanceToTarget);
                CheckIfTargetOutOfRange(distanceToTarget);
            }

            m_character.SetMovementDirection(GetTargetMovementDirection());
        }
    }
}
