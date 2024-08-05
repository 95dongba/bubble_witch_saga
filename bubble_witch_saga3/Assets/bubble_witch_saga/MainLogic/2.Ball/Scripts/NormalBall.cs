using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleWitchSaga.Ball
{
    /// <summary>
    /// 기본 색깔 볼 클래스
    /// </summary>
    public class NormalBall : BallBase
    {
        [SerializeField] private ParticleSystem _skillParticle;
        protected override void ConnectShowEffect()
        {
            SkillStrategy?.ApplySkill();

            if(SkillStrategy != null)
                _skillParticle.gameObject.SetActive(false);            
        }

        protected override void OnInit()
        {
            if(SkillStrategy != null)
                _skillParticle.gameObject.SetActive(true);

            
            ///...
        }
    }
}