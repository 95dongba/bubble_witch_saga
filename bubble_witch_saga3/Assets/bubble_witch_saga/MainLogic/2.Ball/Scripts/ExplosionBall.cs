using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleWitchSaga.Ball
{
    /// <summary>
    /// 기본 색깔 볼 클래스
    /// </summary>
    public class ExplosionBall : BallBase
    {
        [SerializeField] private ParticleSystem _dust_VFX;
        protected override void ConnectShowEffect()
        {
            /// 상하좌우 대각선 볼 삭제
            /// 
            // _dust_VFX.Play();
            Debug.Log("상하좌우 대각선 볼 삭제");
        }

        protected override void OnInit()
        {
            ///...
        }
    }
}