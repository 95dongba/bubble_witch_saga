using BubbleWitchSaga.Ball;
using UnityEngine;

namespace BubbleWitchSaga.Stage
{
    [CreateAssetMenu(fileName = "StageDataScriptable", menuName = "ScriptableData/StageData")]
    public class DataScriptable : ScriptableObject
    {
        /// ------------------
        /// get
        /// ------------------

        public StageConditionData StageCondition => _conditionData;
        
        public EStageType       StageType      => _stageType;
        public EBallSapwnType   BallSapwnType   => _ballSpawnType;

        
        public int Row => _row;
        public int Col => _col;
        public RowData[] BallDatas => _ballDatas;
        public int Remaining_Ball_Count => _remaining_Ball_Count;


        /// ------------------
        /// private
        /// ------------------

        [SerializeField] private StageConditionData _conditionData;
        
        [SerializeField] private EStageType         _stageType;
        [SerializeField] private EBallSapwnType     _ballSpawnType;
        [SerializeField] private int _row;
        [SerializeField] private int _col;
        [SerializeField] private int _remaining_Ball_Count;
        [SerializeField] private RowData[] _ballDatas;
    }
}