using BubbleWitchSaga.Board;
using BubbleWitchSaga.Stage;
using UnityEngine;

namespace BubbleWitchSaga
{
    /// <summary>
    /// 유니티 최초 실행 시 초기화를 수행해 줄 클래스
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        private void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            StageManager.Instance.Init();
            // BoardManager.Instance.Init();
        }
    }
}