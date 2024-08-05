using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace BubbleWitchSaga.Ball
{
    /// <summary>
    /// 볼이 상속받는 베이스 클래스 
    /// </summary>
    public abstract class BallBase : MonoBehaviour
    {
        /// -----------------
        /// get, set
        /// -----------------
        public EBallType BallType { get; private set; }

        /// 행
        public int Row { get; private set; }
        /// 열
        public int Col { get; private set; }

        /// 월드 좌표
        public Vector2 SpawnPosition { get; private set; }

        protected bool IsSkill { get; private set; }


        /// -----------------
        /// private
        /// -----------------
        private EballState  _state;
        private Coroutine   _coShoot;
        private Vector3     _direction;


        /// -----------------
        /// serializedField
        /// -----------------
        [SerializeField] private SpriteRenderer _mainSprite;
        [SerializeField] private Rigidbody2D    _rigidbody2D;
        [SerializeField] private Collider2D     _collider2D;
        [SerializeField] private float          _speed;


        public event Action ConnectedEvent;
        private Action<Vector2> _shootAction;
        private Action<BallBase, string> _returnAction;

        protected IBallSkillStrategy SkillStrategy { get; private set; }


        /// <summary>
        /// 볼 베이스 초기화
        /// </summary>
        protected abstract void OnInit();
        // protected abstract void OnRelease();
        protected abstract void ConnectShowEffect();

        public void ColliderIsEnable(bool isEnable)
        {
            _collider2D.enabled = isEnable;
        }

        public void SetSkillStrategy(IBallSkillStrategy ballSkillStrategy)
        {
            SkillStrategy = ballSkillStrategy;
        }

        public void Init(EBallType ballType, Vector2Int row_col, Vector2 offset, Action<BallBase, string> returnEvent)
        {
            _state                      = EballState.Static;
            _rigidbody2D.bodyType       = RigidbodyType2D.Static;
            _rigidbody2D.gravityScale   = 0;

            BallType            = ballType;
            Row                 = row_col.x;
            Col                 = row_col.y;
            SpawnPosition       = offset;

            ConnectedEvent      = null;
            _returnAction       = returnEvent;
            
            transform.localScale = Vector2.one;

            OnInit();
        }

        public void Shoot(Vector2 dir, Action<Vector2> action)
        {
            if(_coShoot != null)
            {
                StopCoroutine(_coShoot);
                _coShoot = null;
            }

            _shootAction = action;

            _direction = (dir - (Vector2)transform.position).normalized;
            _state = EballState.Move;

            _collider2D.enabled   = true;
            _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            _rigidbody2D.velocity = Vector2.zero;

            _coShoot = StartCoroutine(Co_Shoot());
            
            IEnumerator Co_Shoot()
            {
                while (_state == EballState.Move)
                {
                    _rigidbody2D.velocity = _direction * _speed;
                    yield return null;
                }
                
                _direction              = Vector3.zero;
                _rigidbody2D.velocity   = Vector2.zero;
                _rigidbody2D.bodyType   = RigidbodyType2D.Static;

                _coShoot = null;
                _shootAction?.Invoke(transform.position);
            }
        }

        /// <summary>
        /// 현재 내 위치의 그리드의 행과 열을 저장
        /// </summary>
        public void SetGridOffset(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public void SetSpawnPosition(Vector2 pos)
        {
            SpawnPosition = pos;
        }

        /// <summary>
        /// 볼 릴리즈
        /// </summary>
        public void ReleaseBall()
        {
            _state                      = EballState.Static;
            _rigidbody2D.bodyType       = RigidbodyType2D.Static;
            _rigidbody2D.gravityScale   = 0;

            _returnAction?.Invoke(this, BallType.ToString());
        }

        public void RemoveConnectBall()
        {   
            /// 연결된 볼 삭제 시 해당 볼 효과 추가
            ConnectedEvent?.Invoke();
            ConnectShowEffect();

            gameObject.SetActive(false);

            // TODO : 연출 추가

            //연출 추가 후 풀에 넣기
            // ReleaseBall();
        }

        public void RemoveFloatingBall()
        {
            // TODO : 연출 추가
            ConnectedEvent?.Invoke();
            ConnectShowEffect();

            _rigidbody2D.bodyType       = RigidbodyType2D.Dynamic;
            _rigidbody2D.gravityScale   = 1;
            _collider2D.enabled         = false;
            

            //연출 추가 후 풀에 넣기
            // ReleaseBall();
        }

        private void OnCollisionEnter2D(Collision2D collision2D)
        {
            if(collision2D.gameObject.layer == gameObject.layer || collision2D.gameObject.layer == LayerMask.NameToLayer("FinishWall"))
            {
                _state = EballState.Static;
                return;
            }

            if(collision2D.gameObject.layer == LayerMask.NameToLayer("ReflectionWall"))
            {
                _direction = Vector2.Reflect(_direction, collision2D.GetContact(0).normal);
            }
        }
    }
}