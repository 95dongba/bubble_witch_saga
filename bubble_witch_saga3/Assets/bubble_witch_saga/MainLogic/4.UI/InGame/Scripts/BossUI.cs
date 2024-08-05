using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BubbleWitchSaga.UI.Boss
{
    public class BossUI : MonoBehaviour
    {
        private int _helath;
        private int _curHealth;

        [SerializeField] private Image hpBar_IMG;
        [SerializeField] private Image hpBar_Slide_IMG;

        public void Init(int hp)
        {
            gameObject.SetActive(true);

            _helath     = hp;
            _curHealth  = hp;

            hpBar_Slide_IMG.fillAmount = (float) _curHealth / _helath;
        }

        public void Hit(Action endedStage)
        {
            _curHealth--;
            hpBar_Slide_IMG.fillAmount = (float)_curHealth / _helath;

            if (_curHealth <= 0)
            {
                endedStage?.Invoke();

                Hide();
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

    }
}