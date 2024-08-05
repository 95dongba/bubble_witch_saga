using UnityEngine;

namespace BubbleWitchSaga.Ball
{
    /// <summary>
    /// 각 행에 생성되는 볼 데이터
    /// </summary>
    [System.Serializable]
    public class BallData
    {
        public EBallType BallType;
        // public bool IsSkill;
    }

    [System.Serializable]
    public class RowData
    {
        /// ------------------
        /// get
        /// ------------------        
        public BallData[] Data => _colunmArray;


        /// ------------------
        /// private
        /// ------------------
        [SerializeField] private BallData[] _colunmArray;
    }
}