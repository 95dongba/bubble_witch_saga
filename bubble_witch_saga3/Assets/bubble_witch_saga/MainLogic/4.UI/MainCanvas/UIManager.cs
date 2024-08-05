using System;
using BubbleWitchSaga.Board;
using BubbleWitchSaga.Stage;
using BubbleWitchSaga.UI.Boss;
using BubbleWitchSaga.UI.JoyPad;
using UnityEngine;

namespace BubbleWitchSaga.UI
{
    public class UIManager
    {
        /// ------------------
        /// get, set
        /// ------------------        
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new UIManager();
                    _instance.Init();
                }

                return _instance;
            }
        }

        /// ------------------
        /// const, readonly
        /// ------------------
        private const string INGAME_UI_PATH = "UI/InGameUI";
        private const string MAIN_UI_PATH   = "UI/MainUI";
        private const string BOSS_UI_PATH   = "UI/BossUI";


        /// ------------------
        /// private
        /// ------------------
        private MainUI      _mainUI;
        private InGameUI    _inGameUI;
        private BossUI      _bossUI;

        public void Release()
        {
            _mainUI     = null;
            _inGameUI   = null;
            _bossUI     = null;

            _instance = null;
        }

        private void Init()
        {
            if(_mainUI == null)
            {
                _mainUI = UnityEngine.Object.Instantiate(Resources.Load<MainUI>(MAIN_UI_PATH));
                // UnityEngine.Object.DontDestroyOnLoad(_mainUI);
            }

            if(_inGameUI == null)
            {
                _inGameUI = UnityEngine.Object.Instantiate(Resources.Load<InGameUI>(INGAME_UI_PATH));
                _inGameUI.Init(BoardManager.Instance.SwapBall);

                // UnityEngine.Object.DontDestroyOnLoad(_inGameUI);
            }

            if (_bossUI == null)
            {
                _bossUI = UnityEngine.Object.Instantiate(Resources.Load<BossUI>(BOSS_UI_PATH));
                _bossUI.Hide();

                DataScriptable data = StageManager.Instance.GetStageData(10);
                StageManager.Instance.OnBossStageSetup  += ()   =>  _bossUI.Init(data.StageCondition.Objectives);
                StageManager.Instance.OnBossHit         += ()   =>  _bossUI.Hit(StageManager.Instance.EndStage);

                // UnityEngine.Object.DontDestroyOnLoad(_bossUI);
            }
        }

        public SetupInGameDataContainer SetupInGameData() => new SetupInGameDataContainer();
        public class SetupInGameDataContainer
        {
            public Transform TF_ShootBall  => Instance._inGameUI.TF_ShootBall;
            public Transform TF_ChangeBall => Instance._inGameUI.TF_ChangeBall;

            public void UpdateRemainingBallCount(int count) => Instance._inGameUI.RemainingBallCountText(count);
            public IPad SetPad => Instance._inGameUI.JoyPad;
        }
    }
}