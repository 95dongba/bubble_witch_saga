using System;
using System.Collections;
using System.Collections.Generic;
using BubbleWitchSaga.Ball;
using BubbleWitchSaga.Pool;
using BubbleWitchSaga.Stage;
using BubbleWitchSaga.UI.JoyPad;
using UnityEngine;

namespace BubbleWitchSaga.Board
{
    public interface IBoard
    {
        void Init(DataScriptable data, Action<int> refreshRemaingBall, Transform shoot_TF, Transform change_TF, IPad pad, BossStageSetup bossStage);
        void ReStart();
        void Release();

        void ShootBall(Vector2 dir);
        void SwapBall();
    }

    /// <summary>
    /// 스테이지 클래스
    /// </summary>
    public class GameBoard : MonoBehaviour, IBoard
    {
        /// ------------------
        /// const, readonly
        /// ------------------
        private const float EVEN_OFFSET = 0.5f;
        private const int   CONNECT_COUNT = 3;

        /// ------------------
        /// SerializedField
        /// ------------------
        [SerializeField] private BoardSetupController _setupController;
        [SerializeField] private AnimationCurve _linearCurve;

        /// 월드 좌표에서의 보드판의 생성 위치
        // [SerializeField] private Vector2 _boardOffset;


        /// ------------------
        /// Private
        /// ------------------

        private PoolManager<BallBase> _PoolingBall;

        private BallBase    _shootBall;
        private BallBase    _swapBall;
        private int _remaining_Ball_Count;


        private Transform _shootBall_TF;
        private Transform _SwapBall_TF;

        /// 행 개수
        private int _row;
        /// 열 개수
        private int _col;
    

        /// 보드판의 [행,열] 배열
        private BallBase[,] _board;
        /// 보드판 데이터 
        private RowData[]   _ballDatas;
        private IPad        _pad;

        private Coroutine   _coSwap;


        /// ------------------------
        /// Event, Action, Delegate
        /// ------------------------
        private Action<int> _refreshRemaingBall;

        #region IBoard Interface 
        /// ------------------------
        /// Interface Method
        /// ------------------------
        public void Init(DataScriptable data, Action<int> refreshRemaingBall, Transform shoot_TF, Transform swap_TF, IPad pad, BossStageSetup bossStage)
        {
            /// Setup Data
            _row                    = data.Row;
            _col                    = data.Col;
            _ballDatas              = data.BallDatas;
            _remaining_Ball_Count   = data.Remaining_Ball_Count;

            _shootBall_TF   = shoot_TF;
            _SwapBall_TF    = swap_TF;
            _pad = pad;

            _refreshRemaingBall = refreshRemaingBall;
            _refreshRemaingBall?.Invoke(_remaining_Ball_Count);

            _board          ??= new BallBase[_row, _col];
            _PoolingBall    ??= new PoolManager<BallBase>(transform);

            GeneratorBall();
            
            List<Grid> grids = InitializeBoard();

            _setupController.Init(new BoardSetupController.SetupDataContainer
            (data.BallSapwnType, _PoolingBall, _board, grids, _ballDatas, _row, _col, bossStage));

            _setupController.SpawnBalls(data.BallSapwnType);
            _setupController.StageCondition(data.StageType);

            _shootBall  = _setupController.SetupBall(_shootBall_TF, EBallType.None);
            _swapBall   = _setupController.SetupBall(_SwapBall_TF, _shootBall.BallType);

            _pad.EnableGamepad();
        }

        public void ReStart()
        {
            Release();
            InitializeBoard();
        }

        public void Release()
        {
            // Destroy(gameObject);

            foreach (var item in _board)
                item.ReleaseBall();

            _board = null;
        }


        public void SwapBall()
        {
            if(_coSwap != null)
                return;

            _pad.DisableGamepad();
            _coSwap = StartCoroutine(Co_SwapMove(_shootBall.transform, _swapBall.transform, 0.3f));

            BallBase temp   = _shootBall;
            _shootBall      = _swapBall;
            _swapBall       = temp;

            IEnumerator Co_SwapMove(Transform shoot_TR, Transform swap_TR, float duration)
            {
                float elapsedTime = 0;
                while(elapsedTime < duration)
                {
                    yield return null;

                    elapsedTime += Time.deltaTime;
                    float t = elapsedTime / duration;

                    shoot_TR.position       = Vector2.Lerp(shoot_TR.position,    _SwapBall_TF.position,      _linearCurve.Evaluate(t));
                    shoot_TR.localScale     = Vector2.Lerp(shoot_TR.localScale,  _SwapBall_TF.localScale,    _linearCurve.Evaluate(t));

                    swap_TR.position        = Vector2.Lerp(swap_TR.position,    _shootBall_TF.position,      _linearCurve.Evaluate(t));
                    swap_TR.localScale      = Vector2.Lerp(swap_TR.localScale,  _shootBall_TF.localScale,    _linearCurve.Evaluate(t));
                }

                _coSwap = null;
                _pad.EnableGamepad();
            }
        }
        
        public void ShootBall(Vector2 dir)
        {
            _refreshRemaingBall?.Invoke(_remaining_Ball_Count--);

            // if (_remaining_Ball_Count <= 0)
            //     return;

            _shootBall.Shoot(dir, BallShotCompleted);
        }

        /// ------------------------------------------------------------------------------------------
        /// ------------------------------------------------------------------------------------------
        #endregion



        /// ------------------------
        /// Private Method
        /// ------------------------
        
        /// 볼 오브젝트 풀링
        private void GeneratorBall()
        {
            _PoolingBall.Generator(Resources.Load<BallBase>("Ball/Blue"), EBallType.Blue_Ball.ToString(), 20);
            _PoolingBall.Generator(Resources.Load<BallBase>("Ball/Red"), EBallType.Red_Ball.ToString(), 20);
            _PoolingBall.Generator(Resources.Load<BallBase>("Ball/Yellow"), EBallType.Yellow_Ball.ToString(), 20);
            _PoolingBall.Generator(Resources.Load<BallBase>("Ball/Explosion"), EBallType.Explosion_Ball.ToString(), 20);
            _PoolingBall.Generator(Resources.Load<BallBase>("Ball/Spawn"), EBallType.Spawn_Ball.ToString(), 5);
        }

        /// 스테이지 정보에 따라 보드판 초기화
        private List<Grid> InitializeBoard()
        {
            List<Grid> grids = new List<Grid>();

            for (int c = 0; c < _col; c++)
            {
                // int rowCount = (c % 2 == 0) ? _row : _row - 1;

                for (int r = 0; r < _row; r++)
                {
                    if (_ballDatas[c].Data[r].BallType == EBallType.None)
                        continue;

                    float oddOffset = c % 2 == 0 ? 0 : -EVEN_OFFSET;
                    Vector2 offset  = new Vector2(r - oddOffset, _row - 1 - c);

                    grids.Add(new Grid(offset, r, c));
                }
            }
            
            return grids;
        }

        /// <summary>
        /// 슈팅한 볼의 콜백 함수
        /// </summary>
        private void BallShotCompleted(Vector2 worldPosition)
        {
            Vector2Int row_col = GridFromWorldPosition(worldPosition);

            ShootBallSetGridPosition(row_col);
            ConnectedBalls(row_col);
            
            _shootBall = _setupController.SetupBall(_shootBall_TF, _swapBall.BallType);
        }

        /// <summary>
        /// 보드의 행과 열에 따라 월드 좌표로 역산
        /// </summary>
        private Vector2 GetWorldPositionFromGrid(int row, int col)
        {
            float evenOffset = (col % 2 == 0) ? 0 : EVEN_OFFSET;
            return new Vector2(row + evenOffset, _row - 1 - col);
        }

        /// <summary>
        /// 볼의 월드 좌표 위치에 따라 보드의 행과 열로 역산 함수
        /// </summary>
        private Vector2Int GridFromWorldPosition(Vector2 worldPosition)
        {
            int col = Mathf.RoundToInt(_row - 1 - worldPosition.y);
            float evenOffsetX = worldPosition.x;

            if (col % 2 != 0)
            {
                evenOffsetX -= EVEN_OFFSET;
            }

            int row = Mathf.RoundToInt(evenOffsetX);
            Vector2Int row_col = new Vector2Int(row, col);

            if (!IsWithinBounds(row_col))
            {
                ExpandGrid();

                // 확장 후 재계산
                col = Mathf.RoundToInt(_row - 1 - worldPosition.y);
                evenOffsetX = worldPosition.x;

                if (col % 2 != 0)
                {
                    evenOffsetX -= EVEN_OFFSET;
                }

                row = Mathf.RoundToInt(evenOffsetX);
                return new Vector2Int(row, col);
            }

            return row_col;
        }

        private void ExpandGrid()
        {
            int newColCount = _col + 1;

            BallBase[,] newBoard = new BallBase[_row, newColCount];

            for (int r = 0; r < _row; r++)
            {
                for (int c = 0; c < _col; c++)
                {
                    if(_board[r,c] != null)
                    {
                        // _board[r,c].SetGridOffset(r, c + 1);
                    }

                    newBoard[r, c] = _board[r, c];
                }
            }

            for (int r = 0; r < _row; r++)
            {
                newBoard[r, newColCount - 1] = null;
            }

            _board  = newBoard;
            _col    = newColCount;

            _setupController.SetBoard(_board);
        }


        /// <summary>
        /// 슈팅된 볼이 보드판에 도착했을 때 볼의 위치에 따라 보드판의 그리드에 추가되는 함수
        /// </summary>
        private void ShootBallSetGridPosition(Vector2Int row_col)
        {
            if(_board[row_col.x, row_col.y] != null)
            {
                Debug.LogWarning($"{row_col.x}, {row_col.y}, {_board[row_col.x, row_col.y].name} : 해당 그리드에 볼이 있습니다");

                _shootBall.ReleaseBall();
                return;
            }

            Vector2 gridWorldPosition       = GetWorldPositionFromGrid(row_col.x, row_col.y);
            _shootBall.transform.position   = gridWorldPosition;

            _board[row_col.x, row_col.y] = _shootBall;
            _shootBall.SetGridOffset(row_col.x, row_col.y);
        }

        /// <summary>
        /// 슛을 쏜 공과 연결되어있는 볼들을 찾는 함수
        /// </summary>
        private List<BallBase> FindConnectedBalls(Vector2Int startPosition, EBallType ballType)
        {
            List<BallBase>      connectedBalls      = new List<BallBase>();
            Queue<Vector2Int>   positionsToCheck    = new Queue<Vector2Int>();
            HashSet<Vector2Int> checkedPositions    = new HashSet<Vector2Int>();

            positionsToCheck.Enqueue(startPosition);
            checkedPositions.Add(startPosition);

            while (positionsToCheck.Count > 0)
            {
                Vector2Int  position    = positionsToCheck.Dequeue();
                BallBase    ball        = _board[position.x, position.y];

                if(ball == null || ball.BallType != ballType)
                    continue;

                connectedBalls.Add(ball);

                foreach (Vector2Int neighbor in GetNeighbors(position))
                {
                    if (!checkedPositions.Contains(neighbor) && IsWithinBounds(neighbor))
                    {
                        positionsToCheck.Enqueue(neighbor);
                        checkedPositions.Add(neighbor);
                    }
                }
            }

            return connectedBalls;
        }

        /// <summary>
        /// 폭발하는 공에 닿았을 때 상하좌우 대각선 1칸씩 제거
        /// </summary>
        private void ExplosionBallEffect(Vector2Int hitPoint)
        {
            List<BallBase> explosionBalls = FindExplosionBalls(hitPoint);

            if(explosionBalls.Count <= 0)
                return;

            for (int i = 0; i < explosionBalls.Count; i++)
            {
                Vector2Int explosionPoint = new Vector2Int(explosionBalls[i].Row, explosionBalls[i].Col);

                foreach (Vector2Int neighbor in GetNeighbors(explosionPoint))
                {
                    if (!IsWithinBounds(neighbor))
                        continue;

                    BallBase ball = _board[neighbor.x, neighbor.y];

                    if (ball == null)
                        continue;

                    ball.RemoveConnectBall();
                    explosionBalls[i].RemoveConnectBall();

                    _board[explosionBalls[i].Row, explosionBalls[i].Col] = null;
                    _board[ball.Row, ball.Col] = null;
                }
            }
        }

        private List<BallBase> FindExplosionBalls(Vector2Int hitPosition)
        {
            List<BallBase> explosionBalls = new List<BallBase>();

            foreach (Vector2Int neighbor in GetNeighbors(hitPosition))
            {
                if (!IsWithinBounds(neighbor))
                    continue;

                BallBase neighborBall = _board[neighbor.x, neighbor.y];
                if (neighborBall != null && neighborBall.BallType == EBallType.Explosion_Ball)
                {
                    explosionBalls.Add(neighborBall);
                }
            }

            return explosionBalls;
        }

        /// <summary>
        /// 슈팅한 볼과 연결되어있는 볼 중 같은 타입의 볼을 제거하는 함수
        /// </summary>
        private void ConnectedBalls(Vector2Int startPosition)
        {
            EBallType ballType = _board[startPosition.x, startPosition.y].BallType;
            List<BallBase> connectedBalls = FindConnectedBalls(startPosition, ballType);

            if (connectedBalls.Count >= CONNECT_COUNT)
            {
                foreach (BallBase ball in connectedBalls)
                {
                    ball.RemoveConnectBall();
                    _board[ball.Row, ball.Col] = null;
                }
            }

            ExplosionBallEffect(startPosition);
            RemoveFloatingBalls();

            _setupController.FillEmptyBoard(_pad.EnableGamepad);
        }

        /// <summary>
        /// 좌측 상단 0열을 기준으로 공중에 떠 있는 공 탐색 후 제거
        /// </summary>
        private void RemoveFloatingBalls()
        {
            HashSet<Vector2Int> connectedToSide = new HashSet<Vector2Int>();
            Queue<Vector2Int>   queue   = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            for (int r = 0; r < _row; r++)
            {
                if (_board[r, 0] != null)
                {
                    queue.Enqueue(new Vector2Int(r, 0));
                    visited.Add(new Vector2Int(r, 0));
                }
            }

            while (queue.Count > 0)
            {
                Vector2Int position = queue.Dequeue();
                connectedToSide.Add(position);

                foreach (Vector2Int neighbor in GetNeighbors(position))
                {
                    if (!visited.Contains(neighbor) && IsWithinBounds(neighbor) && _board[neighbor.x, neighbor.y] != null)
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }

            // 공중에 떠 있는 모든 버블 제거
            for (int r = 0; r < _row; r++)
            {
                for (int c = 0; c < _col; c++)
                {
                    Vector2Int position = new Vector2Int(r, c);
                    if (!connectedToSide.Contains(position) && _board[r, c] != null)
                    {
                        _board[r, c].RemoveFloatingBall();
                        _board[r, c] = null;
                    }
                }
            }
        }



        /// <summary>
        /// 설정된 그리드 값 안에 있는지 확인하는 함수
        /// </summary>
        private bool IsWithinBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x < _row && position.y >= 0 && position.y < _col;
        }

        /// <summary>
        /// 해당 그리드의 위치에서 직선, 대각선의 그리드를 리턴하는 함수
        /// </summary>
        private IEnumerable<Vector2Int> GetNeighbors(Vector2Int position)
        {
            yield return new Vector2Int(position.x, position.y + 1);
            yield return new Vector2Int(position.x, position.y - 1);
            yield return new Vector2Int(position.x + 1, position.y);
            yield return new Vector2Int(position.x - 1, position.y);
            
            if(position.y % 2 == 0)
            {
                yield return new Vector2Int(position.x - 1 , position.y + 1);
                yield return new Vector2Int(position.x + 1 , position.y - 1);

                yield return new Vector2Int(position.x + 1, position.y + 1);
                yield return new Vector2Int(position.x - 1, position.y - 1);
            }
            else
            {
                yield return new Vector2Int(position.x + 1, position.y + 1);
                yield return new Vector2Int(position.x + 1, position.y - 1);

                yield return new Vector2Int(position.x + 1, position.y - 1);
                yield return new Vector2Int(position.x - 1, position.y - 1);
            }
        }
    }
}