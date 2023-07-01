using Unity.Mathematics;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    class FollowMoveDirection : MonoBehaviour
    {
        // 따라갈 방향을 정의하는 열거형
        public enum EFollowStrategy
        {
            FlipSprites,  // 스프라이트 뒤집기
            NegativeScale // 스케일 반전
        }
        
        // 인스펙터에서 설정 가능한 변수들
        [SerializeField] private CharacterBase m_target = null;  // 따라갈 대상
        [SerializeField] private EFollowStrategy m_strategy = EFollowStrategy.FlipSprites;  // 따라갈 전략
        [SerializeField] private SpriteRenderer[] m_toFlip = null;  // 뒤집을 스프라이트들

        // 프라이빗 멤버 변수
        private Vector3 m_initialPosition;  // 초기 위치

        public void Awake()
        {
            // 따라갈 대상이 있다면, 그 대상의 방향 변경 이벤트를 구독합니다.
            if (m_target != null)
            {
                m_target.directionChangedEvent.AddListener(OnTargetDirectionChanged);
            }
            
            // 초기 위치를 저장합니다.
            m_initialPosition = transform.localPosition;
        }

        // 따라갈 대상의 방향이 바뀌었을 때 호출될 함수입니다.
        public void OnTargetDirectionChanged(EDirection direction)
        {
            // 방향이 오른쪽이면 1, 왼쪽이면 -1을 곱합니다.
            float modifier = direction == EDirection.Right ? 1.0f : -1.0f;

            // 따라갈 전략이 스프라이트 뒤집기라면,
            if (m_strategy == EFollowStrategy.FlipSprites)
            {
                // 이 오브젝트의 위치를 변경합니다.
                transform.localPosition = new Vector3(m_initialPosition.x * modifier, m_initialPosition.y, m_initialPosition.z);

                // 뒤집을 스프라이트가 설정되어 있다면,
                if (m_toFlip != null)
                {
                    // 각 스프라이트를 뒤집습니다.
                    foreach (SpriteRenderer spriteRenderer in m_toFlip)
                    {
                        spriteRenderer.flipX = direction == EDirection.Left;
                    }
                }
            }
            // 따라갈 전략이 스케일 반전이라면,
            else
            {
                // 이 오브젝트의 스케일을 변경합니다.
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * modifier, transform.localScale.y, transform.localScale.z);
            }
        }
    }
}
