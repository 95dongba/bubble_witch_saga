using UnityEngine;

namespace BubbleWitchSaga.Stage
{
    public interface IData
    {
        public DataScriptable GetStageData { get; }

        void Save();
        void Load();
    }

    public class StageData : IData
    {
        public DataScriptable GetStageData => ScriptableData;

        private DataScriptable dataScriptable;


        private Data _data;



        public DataScriptable ScriptableData
        {
            get
            {
                if(dataScriptable == null)
                {
                    dataScriptable = Resources.Load<DataScriptable>("stage");
                }

                return dataScriptable;
            }
        }

        public void Load()
        {
            
        }

        public void Save()
        {
            
        }


        /// ----------------
        /// Local Class
        /// ----------------

        private class Data
        {
            public int Stage;
            public int Score;
            public int High_Score;

            public void Save()
            {

            }

            public void Load()
            {

            }
        }
    }
}