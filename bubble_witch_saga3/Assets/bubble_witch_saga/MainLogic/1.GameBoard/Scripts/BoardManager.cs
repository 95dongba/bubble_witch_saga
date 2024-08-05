using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BubbleWitchSaga.Ball;
using BubbleWitchSaga.Stage;
using BubbleWitchSaga.UI;

namespace BubbleWitchSaga.Board
{
    /// <summary>
    /// 스테이지 관리 매니저
    /// </summary>
    public class BoardManager
    {
        /// ------------------
        /// get, set
        /// ------------------
        private static BoardManager _instance;
        public static BoardManager Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new BoardManager();

                return _instance;
            }
        }

        /// ------------------
        /// const, readonly
        /// ------------------
        private const string BOARD_PATH = "Board/OBJ_GameBoard";



        /// ------------------
        /// private
        /// ------------------
        private IBoard _board;


        
        /// <summary>
        /// 스테이지 초기화
        /// </summary>
        public void Init()
        {
            _board = Object.Instantiate(Resources.Load<GameBoard>(BOARD_PATH));               

            UIManager.SetupInGameDataContainer setupInGameData = UIManager.Instance.SetupInGameData();

            if(setupInGameData == null)
                Debug.Log("NULL");

            _board.Init(StageManager.Instance.GetStageData(1),
                        setupInGameData.UpdateRemainingBallCount,
                        setupInGameData.TF_ShootBall,
                        setupInGameData.TF_ChangeBall,
                        setupInGameData.SetPad,
                        StageManager.Instance.SetupBossStage);
        }

        /// <summary>
        /// 현재 스테이지를 재시작
        /// </summary>
        public void ReStart() => Instance._board.ReStart();

        /// <summary>
        /// 현재 스테이지를 제거
        /// </summary>
        public void Release()
        {
            // _board.Release();
            
            _board      = null;
            _instance   = null;
        }

        /// <summary>
        /// 볼 스왑
        /// </summary>

        public void SwapBall() => Instance._board.SwapBall();
        /// <summary>
        /// 볼 슈팅
        /// </summary>
        public void ShootBall(Vector2 dir) => Instance._board.ShootBall(dir);
    }
}