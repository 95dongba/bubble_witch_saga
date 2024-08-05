using BubbleWitchSaga.Ball;
using UnityEngine;

namespace BubbleWitchSaga.Stage
{
    [CreateAssetMenu(fileName = "StageConditionData", menuName = "ScriptableData/StageCondition")]
    public class StageConditionData : ScriptableObject
    {
        /// ------------------
        /// get
        /// ------------------


        //스테이지 목표 카운트
        public int Objectives => _objectives;

        
        [SerializeField] private int _objectives;
    }
}