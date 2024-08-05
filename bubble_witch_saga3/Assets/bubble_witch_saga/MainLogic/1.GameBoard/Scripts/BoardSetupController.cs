using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BubbleWitchSaga.Ball;
using BubbleWitchSaga.Pool;
using BubbleWitchSaga.Stage;
using UnityEngine;

namespace BubbleWitchSaga.Board
{
    public class BoardSetupController : MonoBehaviour
    {
        [SerializeField] private AnimationCurve _linearCurve;
        private SetupDataContainer _setupData;

        private Dictionary<EStageType, Action>      _stageConditionDict;
        private Dictionary<EBallSapwnType, Action>  _ballSpawnDict;

        private readonly WaitForSeconds BALL_SPAWN_DELAY = new WaitForSeconds(0.15f);

        public void Init(SetupDataContainer setupDataContainer)
        {
            _setupData ??= setupDataContainer;

            _ballSpawnDict ??= new Dictionary<EBallSapwnType, Action>()
            {
                 { EBallSapwnType.Default,      DefaultSpawn },
                 { EBallSapwnType.Half_Sequence,Half_Sequence_SpawnBall }
            };

            _stageConditionDict ??= new Dictionary<EStageType, Action>()
            {
                { EStageType.Stage_Boss, _setupData.OnBossStageSetup.Invoke },
            };
        }


        public BallBase SetupBall(Transform ball_TR, EBallType shootBall)
        {
            EBallType shootBall_Type = GetRandomBall();

            BallBase ball = _setupData.Pool.Spawn(shootBall_Type.ToString(), ball_TR.position);
            ball.ColliderIsEnable(false);

            ball.transform.localScale = ball_TR.localScale;

            ball.Init(shootBall_Type, Vector2Int.zero, Vector2.zero,
            _setupData.Pool.Return);

            return ball;


            EBallType GetRandomBall()
            {
                EBallType[] randomShootBallTypes = new EBallType[] { EBallType.Blue_Ball, EBallType.Red_Ball, EBallType.Yellow_Ball };

                EBallType selectedBall = randomShootBallTypes[UnityEngine.Random.Range(0, randomShootBallTypes.Length)];

                if (selectedBall == shootBall)
                    return GetRandomBall();
                
                return selectedBall;
            }
        }

        public void StageCondition(EStageType type)
        {
            if (_stageConditionDict == null)
            {
                Debug.LogError($"초기화되지않은 딕셔너리 사용 Class : {typeof(BoardSetupController)} : _stageConditionDict");
                return;
            }

            if (!_stageConditionDict.TryGetValue(type, out var action))
                return;

            action?.Invoke();
        }
    
        public void SpawnBalls(EBallSapwnType type)
        {
            if(_ballSpawnDict == null)
            {
                Debug.LogError($"초기화되지않은 딕셔너리 사용 Class : {typeof(BoardSetupController)} : _setupDict");
                return;
            }

            if(!_ballSpawnDict.TryGetValue(type, out var action))
                return;

            action?.Invoke();
        }

        public void FillEmptyBoard(Action donecallback)
        {
            switch(_setupData.BallSapwnType)
            {
                case EBallSapwnType.Default: break;

                case EBallSapwnType.Half_Sequence :
                    StartCoroutine(Fill_Half_SequenceBall(donecallback));
                    break;
            } 
        }

        private void DefaultSpawn()
        {
            for (int i = 0; i < _setupData.Grids.Count; i++)
            {
                Grid grid = _setupData.Grids[i];

                Vector2 offset = grid.Offset;
                int row = grid.Row;
                int col = grid.Col;

                EBallType   ballType    = _setupData.RowDatas[col].Data[row].BallType;
                BallBase    ball        = _setupData.Pool.Spawn(ballType.ToString(), offset);

                ball.Init(ballType, new Vector2Int(row, col), offset, _setupData.Pool.Return);

                _setupData.Board[row, col] = ball;
            }
        }
    
        private IEnumerator Fill_Half_SequenceBall(Action donecallback)
        {
            // 좌 우 데이터 튜플
            (Vector2 leftSpawnPos, Vector2 rightSpawnPos)   spawnPositions      = (Vector2.zero, Vector2.zero);
            (List<Grid> leftGrids, List<Grid> rightGrids)   grids               = (new List<Grid>(), new List<Grid>());
            (int leftEmpty, int rightEmpty)                 emptyBalls          = (0, 0);

            int half = _setupData.Row / 2;

            for (int i = 0; i < _setupData.Grids.Count; i++)
            {
                Grid grid = _setupData.Grids[i];

                Vector2 offset  = grid.Offset;
                int     row     = grid.Row;
                int     col     = grid.Col;

                if (_setupData.RowDatas[col].Data[row].BallType != EBallType.Spawn_Ball)
                {
                    if (grid.Row < half)    grids.leftGrids.Add(grid);
                    else                    grids.rightGrids.Add(grid);
                }

                if(_setupData.RowDatas[col].Data[row].BallType == EBallType.Spawn_Ball)
                {
                    Vector2 spawnPosition = _setupData.Board[row, col].SpawnPosition;

                    if (grid.Row < half)    spawnPositions.leftSpawnPos    = spawnPosition;
                    else                    spawnPositions.rightSpawnPos   = spawnPosition;
                }

                if(_setupData.Board[row,col] == null)
                {
                    if(grid.Row < half) emptyBalls.leftEmpty++;
                    else                emptyBalls.rightEmpty++;
                }
            }

            SortGrid(grids.leftGrids, 0);
            SortGrid(grids.rightGrids, 2);

            int maximum = emptyBalls.leftEmpty > emptyBalls.rightEmpty ? emptyBalls.leftEmpty : emptyBalls.rightEmpty;

            for(int i = 0; i < maximum; i++)
            {
                Coroutine leftCoroutine     = null;
                Coroutine rightCoroutine    = null;

                if(i < emptyBalls.leftEmpty)
                    leftCoroutine = StartCoroutine(ProcessShiftBalls(grids.leftGrids, spawnPositions.leftSpawnPos));

                if(i < emptyBalls.rightEmpty)
                    rightCoroutine = StartCoroutine(ProcessShiftBalls(grids.rightGrids, spawnPositions.rightSpawnPos));

                yield return leftCoroutine;
                yield return rightCoroutine;
            }

            donecallback?.Invoke();

            /// --------------------------
            /// Local Method
            /// --------------------------
            
            IEnumerator Co_ShiftBalls(List<Grid> list, Vector2 spawnPos)
            {
                List<Coroutine> moveCoroutines = new List<Coroutine>();

                EBallType[] randomSpawnBallType = new EBallType[]
                 {
                    EBallType.Blue_Ball,
                    EBallType.Red_Ball,
                    EBallType.Yellow_Ball,
                    EBallType.Explosion_Ball,
                 };

                int choose = Choose(new int[] { 10, 10, 10, 1 });

                EBallType   randomSpawnType     = randomSpawnBallType[choose];
                Grid        firstGrid           = list[0];
                BallBase    spawnBall           = _setupData.Pool.Spawn(randomSpawnType.ToString(), spawnPos);

                if (spawnBall.BallType != EBallType.None || spawnBall.BallType != EBallType.Explosion_Ball)
                {
                    int bossHitBall = Choose(new int[] { 1, 15 });
                    if (bossHitBall == 0) spawnBall.SetSkillStrategy(new BossHitSkill());
                }

                spawnBall.Init(randomSpawnType,
                new Vector2Int(firstGrid.Row, firstGrid.Col),
                firstGrid.Offset,
                _setupData.Pool.Return);

                BallBase[,] tempBoard = new BallBase[_setupData.Board.GetLength(0), _setupData.Board.GetLength(1)];
                Array.Copy(_setupData.Board, tempBoard, _setupData.Board.Length);

                for (int i = list.Count - 1; i > 0; i--)
                {
                    Grid currentGrid = list[i];
                    Grid prevGrid = list[i - 1];

                    BallBase currentBall = _setupData.Board[prevGrid.Row, prevGrid.Col];
                    if (currentBall != null)
                    {
                        tempBoard[currentGrid.Row, currentGrid.Col] = currentBall;
                        currentBall.SetGridOffset(currentGrid.Row, currentGrid.Col);
                        currentBall.SetSpawnPosition(currentGrid.Offset);
                        moveCoroutines.Add(StartCoroutine(Co_MoveBall(currentBall, list, i)));
                    }
                }

                spawnBall.SetGridOffset(firstGrid.Row, firstGrid.Col);
                spawnBall.SetSpawnPosition(firstGrid.Offset);
                tempBoard[firstGrid.Row, firstGrid.Col] = spawnBall;

                moveCoroutines.Add(StartCoroutine(Co_MoveBall(spawnBall, list, 0)));

                Array.Copy(tempBoard, _setupData.Board, tempBoard.Length);
                _setupData.Board[firstGrid.Row, firstGrid.Col] = spawnBall;

                foreach (var coroutine in moveCoroutines)
                {
                    yield return coroutine;
                }
            }

            IEnumerator ProcessShiftBalls(List<Grid> list, Vector2 spawnPos)
            {
                yield return StartCoroutine(Co_ShiftBalls(list, spawnPos));
            }
        }

        int Choose(int[] probs)
        {

            float total = 0;

            foreach (float elem in probs)
            {
                total += elem;
            }

            float randomPoint = UnityEngine.Random.value * total;

            for (int i = 0; i < probs.Length; i++)
            {
                if (randomPoint < probs[i])
                {
                    return i;
                }
                else
                {
                    randomPoint -= probs[i];
                }
            }
            return probs.Length - 1;
        }


        private void Half_Sequence_SpawnBall()
        {
            Vector2 leftSpawnPos    = Vector2.zero;
            Vector2 rightSpawnPos   = Vector2.zero;

            List<Grid>  lefts   = new List<Grid>();
            List<Grid>  rights  = new List<Grid>();

            int half = _setupData.Row / 2;
            for(int i = 0; i < _setupData.Grids.Count; i++)
            {
                Grid grid = _setupData.Grids[i];

                Vector2 offset  = grid.Offset;
                int     row     = grid.Row;
                int     col     = grid.Col;

                if(_setupData.RowDatas[col].Data[row].BallType != EBallType.Spawn_Ball)
                {
                    if (grid.Row < half)    lefts.Add(grid);
                    else                    rights.Add(grid);
                }

                if(_setupData.RowDatas[col].Data[row].BallType == EBallType.Spawn_Ball)
                {
                    EBallType   ballType    = _setupData.RowDatas[col].Data[row].BallType;
                    BallBase    ball        = _setupData.Pool.Spawn(EBallType.Spawn_Ball.ToString(), offset);
                    // bool        IsSkill     = _setupData.RowDatas[col].Data[row].IsSkill;

                    ball.Init(ballType, new Vector2Int(row, col), offset, _setupData.Pool.Return);
                    _setupData.Board[row, col] = ball;

                    if (grid.Row < half)    leftSpawnPos    = ball.transform.position;
                    else                    rightSpawnPos   = ball.transform.position;
                }
            }

            SortGrid(lefts, 0);
            SortGrid(rights, 2);

            StartCoroutine(Co_Half_Sequence_SpawnBall(lefts, leftSpawnPos));
            StartCoroutine(Co_Half_Sequence_SpawnBall(rights, rightSpawnPos));


            /// ------------------------
            /// Local Funcion
            /// ------------------------
            
            IEnumerator Co_Half_Sequence_SpawnBall(List<Grid> list, Vector2 spawnPos)
            {
                for(int i =  list.Count - 1; i >= 0; i--)
                {
                    EBallType   ballType    = _setupData.RowDatas[list[i].Col].Data[list[i].Row].BallType;
                    BallBase    ball        = _setupData.Pool.Spawn(ballType.ToString(), spawnPos);

                    if(ball.BallType != EBallType.None || ball.BallType != EBallType.Explosion_Ball || ball.BallType != EBallType.Spawn_Ball)
                    {
                        int bossHitBall = Choose(new int[] { 1, 15 });
                        if (bossHitBall == 0) ball.SetSkillStrategy(new BossHitSkill());
                    }

                    ball.Init(ballType, new Vector2Int(list[i].Row, list[i].Col), list[i].Offset, _setupData.Pool.Return);

                    _setupData.Board[list[i].Row, list[i].Col] = ball;

                    StartCoroutine(Co_MoveBall(ball, list, 0));
                    yield return BALL_SPAWN_DELAY;
                }
            }    
        }

        private IEnumerator Co_MoveBall(BallBase targetBall, List<Grid> grids, int index)
        {
            Grid currentGrid = grids[index];

            Vector2     nextPos = currentGrid.Offset;
            Vector2     endPos  = targetBall.SpawnPosition;
            Transform   ballTR  = targetBall.transform;

            if (Vector2.SqrMagnitude(targetBall.SpawnPosition - endPos) > 0.1f)
                yield break;

            float magnitude = Vector2.SqrMagnitude(nextPos - (Vector2)ballTR.position);
            float elapsedTime = 0f;

            while (magnitude > 0.01f)
            {
                magnitude = Vector2.SqrMagnitude(nextPos - (Vector2)ballTR.position);
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / 0.25f;

                targetBall.transform.position = Vector2.Lerp(targetBall.transform.position, nextPos, _linearCurve.Evaluate(t));
                yield return null;
            }
            
            targetBall.transform.position = nextPos;

            if(nextPos != endPos)
            {
                index++;
                yield return StartCoroutine(Co_MoveBall(targetBall, grids, index));
            }
        }

        private void SortGrid(List<Grid> list, int remainder)
        {
            Dictionary<int, List<int>> keyValuePairs = new Dictionary<int, List<int>>();

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].Col % 4 == remainder)
                {
                    if (!keyValuePairs.TryGetValue(list[i].Col, out var indices))
                    {
                        indices = new List<int>();
                        keyValuePairs[list[i].Col] = indices;
                    }

                    indices.Add(i);
                }
            }

            foreach (var pair in keyValuePairs)
            {
                int startIndex = pair.Value.First();
                int count = pair.Value.Count;

                if (count > 1)
                {
                    list.Reverse(startIndex - count + 1, count);
                }
            }
        }

        private (T1 left, T2 right) GeneratorTuple<T1, T2>()
        {
            T1 left     = default(T1);
            T2 right    = default(T2);


            return (left, right);
        }


        public void SetBoard(BallBase[,] board) => _setupData.SetBoard(board);

        public class SetupDataContainer
        {
            public EBallSapwnType BallSapwnType { get; private set; }

            public BallBase[,]  Board       { get; private set; }
            public List<Grid>   Grids       { get; private set; }
            public RowData[]    RowDatas    { get; private set; }


            public PoolManager<BallBase> Pool { get; private set; }

            public int Row { get; private set; }
            public int Col { get; private set; }


            public  BossStageSetup OnBossStageSetup;

            public SetupDataContainer(EBallSapwnType ballSapwnType, PoolManager<BallBase> pool, BallBase[,] board, List<Grid> grids, RowData[] rowDatas, int row, int col, BossStageSetup bossStage)
            {
                BallSapwnType = ballSapwnType;
                
                Pool        = pool;
                Board       = board;
                Grids       = grids;
                RowDatas    = rowDatas;
                
                Row = row;
                Col = col;

                OnBossStageSetup = bossStage;
            }

            public void SetBoard(BallBase[,] board)
            {
                Board = board;
            }
        }
    }

    public struct Grid
    {
        public Vector2 Offset   { get; private set; }
        public int Row          { get; private set; }
        public int Col          { get; private set; }

        public Grid(Vector2 offset, int row, int col)
        {
            Offset  = offset;
            Row     = row;
            Col     = col;
        }
    } 
}