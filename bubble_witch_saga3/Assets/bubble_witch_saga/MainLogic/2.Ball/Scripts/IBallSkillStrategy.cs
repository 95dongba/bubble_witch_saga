using System.Collections;
using System.Collections.Generic;
using BubbleWitchSaga.Stage;
using BubbleWitchSaga.UI;
using UnityEngine;

namespace BubbleWitchSaga.Ball
{
    public interface IBallSkillStrategy
    {
        void ApplySkill();
    }

    public class DefaultSkill : IBallSkillStrategy
    {
        public void ApplySkill()
        {
            
        }
    }

    public class BossHitSkill : IBallSkillStrategy
    {
        public void ApplySkill()
        {
            StageManager.Instance.Hit();
        }
    }
}

