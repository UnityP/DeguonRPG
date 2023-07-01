using System;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class AIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterBase m_character = null; // AI 캐릭터 참조

        
        [Header("Target Following Settings")]
        [SerializeField][Min(1.0f)] private float m_detectionRadius = 5.0f; // 타겟 탐지 반경
        [SerializeField][Min(1.0f)] private float m_resetFromInitialPositionRadius = 10.0f; // 초기 위치로부터의 재타겟팅 반경
        [SerializeField][Min(1.0f)] private float m_resetFromTargetDistanceRadius = 10.0f; // 타겟으로부터의 재타겟팅 반경
        [SerializeField][Min(0.5f)] private float m_retargetCooldown = 3.0f; // 재타겟팅 쿨다운
        [SerializeField][Min(0.1f)] private float m_soughtDistanceFromTarget = 1.0f; // 타겟 추적 거리

        [Header("Attack Settings")]
        [SerializeField] public float m_attackTriggerRadius = 1.0f; // 공격 시작 반경
        [SerializeField] public float m_attackCooldown = 1.0f; // 공격 쿨다운

        
        private Vector2 m_initialPosition; // AI 캐릭터의 초기 위치
        private Transform m_target = null; // AI 캐릭터의 목표
        private float m_retargetCooldownTimer = 0.0f; // 재타겟팅 쿨다운 타이머
        private float m_attackCooldownTimer = 0.0f; // 공격 쿨다운 타이머


        private void Awake() // 객체 생성 시 초기화
        {
            m_initialPosition = transform.position; // 초기 위치 설정
        }
        
        private Transform FindTarget() // 타겟 찾기
        {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, m_detectionRadius, Vector2.zero, 0.0f); // 일정 반경 내의 모든 객체 검색

            foreach (RaycastHit2D hit in hits) // 검색된 모든 객체를 순회
            {
                if (hit.transform.TryGetComponent(out CharacterBase character) && character.CanBeAttackedBy(m_character)) // 공격 가능한 캐릭터가 있는지 검사
                {
                    return hit.transform; // 공격 가능한 캐릭터를 타겟으로 설정
                }
            }

            return null; // 공격 가능한 캐릭터가 없으면 null 반환
        }

        private Vector2 GetTargetPosition() // 타겟의 위치를 가져옴
        {
            return m_target ? (Vector2)m_target.position : m_initialPosition; // 타겟이 있으면 타겟의 위치, 없으면 초기 위치 반환
        }

        private Vector2 GetTargetMovementDirection() // 타겟의 이동 방향을 가져옴
        {
            Vector2 targetPosition = GetTargetPosition(); // 타겟의 위치를 가져옴
            Vector2 currentPosition = transform.position; // 현재 위치를 가져옴
            Vector2 targetMovementDirection = targetPosition - currentPosition; // 타겟의 이동 방향을 계산

            if (targetMovementDirection.magnitude > m_soughtDistanceFromTarget) // 타겟과의 거리가 설정된 추적 거리보다 클 경우
            {
                targetMovementDirection.Normalize(); // 이동 방향을 정규화하여 반환
                return targetMovementDirection;
            }

            return Vector2.zero; // 그렇지 않으면 정지
        }

        private void UpdateCooldowns() // 쿨다운 업데이트
        {
            if (m_retargetCooldownTimer > 0.0f) // 재타겟팅 쿨다운이 0보다 크면
            {
                m_retargetCooldownTimer = Math.Max(m_retargetCooldownTimer - Time.fixedDeltaTime, 0.0f); // 재타겟팅 쿨다운을 감소시킴
            }

            if (m_attackCooldownTimer > 0.0f) // 공격 쿨다운이 0보다 크면
            {
                m_attackCooldownTimer = Math.Max(m_attackCooldownTimer - Time.fixedDeltaTime, 0.0f); // 공격 쿨다운을 감소시킴
            }
        }

        private void TryToAttackTarget(float distanceToTarget) // 타겟 공격 시도
        {
            // 공격 쿨다운이 0이고, 타겟과의 거리가 공격 반경보다 작으면
            if (m_attackCooldownTimer == 0.0f && distanceToTarget < m_attackTriggerRadius)
            {
                // 캐릭터에서 사용할 수 있는 첫 번째 트리거 가능 능력을 찾아 발사합니다.
                foreach (AbilityBase ability in m_character.abilityInstances) // 캐릭터의 모든 능력을 순회
                {
                    if (ability is ITriggerableAbility) // 발동 가능한 능력이 있는지 확인
                    {
                        m_character.FireAbility((ITriggerableAbility)ability); // 발동 가능한 능력 발사
                        m_attackCooldownTimer = m_attackCooldown; // 공격 쿨다운 재설정
                        break;
                    }
                }
            }
        }

        private void CheckIfTargetOutOfRange(float distanceToTarget) // 타겟이 사정거리를 벗어났는지 확인
        {
            float distanceToInitialPosition = Vector2.Distance(m_initialPosition, transform.position); // 초기 위치와의 거리 계산
            bool isTooFarFromInitialPosition = distanceToInitialPosition > m_resetFromInitialPositionRadius; // 초기 위치로부터 너무 멀리 떨어졌는지 확인
            bool isTooFarFromTarget = distanceToTarget > m_resetFromTargetDistanceRadius; // 타겟으로부터 너무 멀리 떨어졌는지 확인

            if (isTooFarFromInitialPosition || isTooFarFromTarget) // 초기 위치나 타겟으로부터 너무 멀리 떨어진 경우
            {
                m_retargetCooldownTimer = m_retargetCooldown; // 재타겟팅 쿨다운 재설정
                m_target = null; // 타겟 제거
            }
        }

        private void FixedUpdate() // 고정 업데이트 함수 (물리 계산용)
        {
            UpdateCooldowns(); // 쿨다운 업데이트

            if (!m_target) // 타겟이 없는 경우
            {
                if (m_retargetCooldownTimer == 0.0f) // 재타겟팅 쿨다운이 0인 경우
                {
                    m_target = FindTarget(); // 타겟 찾기
                    if (m_target) // 타겟이 있는 경우
                    {
                        GameManager.NotificationSystem.targetDetected.Invoke(this, m_target); // 타겟 발견 알림 발생
                    }
                }
            }
            else // 타겟이 있는 경우
            {
                float distanceToTarget = Vector2.Distance(m_target.position, transform.position); // 타겟과의 거리 계산

                TryToAttackTarget(distanceToTarget); // 타겟 공격 시도
                CheckIfTargetOutOfRange(distanceToTarget); // 타겟이 사정거리를 벗어났는지 확인
            }

            m_character.SetMovementDirection(GetTargetMovementDirection()); // 캐릭터의 이동 방향 설정
        }
    }
}
