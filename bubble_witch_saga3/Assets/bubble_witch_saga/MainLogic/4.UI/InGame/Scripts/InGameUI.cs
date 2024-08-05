using System;
using BubbleWitchSaga.Board;
using BubbleWitchSaga.UI.JoyPad;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BubbleWitchSaga.UI
{
    public class InGameUI : MonoBehaviour
    {
        /// ------------------
        /// get, set
        /// ------------------        
        public Transform TF_ShootBall   => _shootBall_RTF;
        public Transform TF_ChangeBall  => _SwapBall_RTF;


        public Pad JoyPad => _joyPad;
        
        /// ------------------
        /// private
        /// ------------------


        /// ------------------
        /// SerializeField
        /// ------------------
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Pad _joyPad;
        [SerializeField] private TextMeshProUGUI _ballCount_TMP;

        [SerializeField] private RectTransform _shootBall_RTF;
        [SerializeField] private RectTransform _SwapBall_RTF;

        
        [SerializeField] private Button _changeBall_BTN;

        [Space(10)][SerializeField] private SpecialBallUI _special_Ball_UI;

        /// <summary>
        /// 인게임 UI 초기화
        /// </summary>
        public void Init(Action swapAction)
        {
            SetupCanvas();
            SetupPad();

            _changeBall_BTN.onClick.AddListener(() => swapAction.Invoke());
        }

        public void RemainingBallCountText(int remaining)
        {
            _ballCount_TMP.text = $"{remaining}";
        }




        private void SetupCanvas()
        {
            _canvas.renderMode  = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = Camera.main;
        }

        private void SetupPad()
        {
            _joyPad.Init(TF_ShootBall.position, BoardManager.Instance.ShootBall);
        }



        /// <summary>
        /// SpecialBall UI 관리 클래스
        /// </summary>
        [System.Serializable]
        private class SpecialBallUI
        {
            private const float MAX_VALUE       = 12;
            private const float GAUGE_PERCENT   = 4;

            public float Current_Value { get; private set; }
            public float Max_Value { get; private set; }

            public Image    Fill_IMG;
            public Button   Fill_BTN;

            public void Init(Action specialBallEvent)
            {
                Current_Value       = 0;
                Max_Value           = MAX_VALUE;
                Fill_IMG.fillAmount = 0;

                Fill_BTN.onClick.RemoveAllListeners();
                Fill_BTN.onClick.AddListener(() => ClickEvent(specialBallEvent));
            }

            private void ClickEvent(Action specialBallEvent)
            {
                Current_Value       += GAUGE_PERCENT;
                Fill_IMG.fillAmount = Current_Value / Max_Value;

                if (Max_Value > Current_Value)
                    return;

                specialBallEvent?.Invoke();
            }
        }        
    }
}