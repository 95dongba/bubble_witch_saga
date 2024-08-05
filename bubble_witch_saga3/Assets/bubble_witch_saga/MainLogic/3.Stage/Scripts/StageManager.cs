using System;
using System.Collections;
using System.Collections.Generic;
using BubbleWitchSaga.Board;
using BubbleWitchSaga.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BubbleWitchSaga.Stage
{
    public delegate void BossStageSetup();
    public delegate void BossHit();

    public class StageManager
    {
        private const string VIEW_PATH = "Stage/StageView";

        /// ------------------
        /// get, set
        /// ------------------
        private static StageManager _instance;
        public static StageManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new StageManager();
                    _instance.Init();
                }

                return _instance;
            }
        }

        /// ------------------
        /// private
        /// ------------------
        private IData       _data;
        private IView       _view;
        


        public event BossStageSetup OnBossStageSetup;
        public event BossHit        OnBossHit;

        public void SetupBossStage()
        {
            OnBossStageSetup += BossStage;
            OnBossStageSetup?.Invoke();
        }

        private void BossStage()
        {
            
        }

        public void Hit()
        {
            OnBossHit?.Invoke();
        }


        public void EndStage()
        {
            _view.ShowEndView();
        }



        public void Init()
        {
            if(_data == null)
            {
                _data = new StageData();
                _data.Load();
            }

            if(_view == null)
            {
                Action[] actions = new Action[]
                {
                    Game,
                    End
                };

                _view = UnityEngine.Object.Instantiate(Resources.Load<StageView>(VIEW_PATH));
                _view.Init(actions);
            }

            _view.ShowStartView();
        }

        private void Game()
        {
            BoardManager.Instance.Init();
        }

        private void End()
        {
            Debug.Log("END");
            
            UIManager.Instance.Release();
            BoardManager.Instance.Release();

        }

        public DataScriptable GetStageData(int level)
        {
            if(_data == null)
            {
                Debug.LogError($"생성된 데이터가 없습니다, {typeof(StageManager)}");
                return null;
            }

            return _data.GetStageData;
        }

    }
}