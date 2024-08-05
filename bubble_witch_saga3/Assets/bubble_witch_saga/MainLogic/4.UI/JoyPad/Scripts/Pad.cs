using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BubbleWitchSaga.UI.JoyPad
{
    public enum EGamePadState
    {
        InUse,
        Disabled,
    }
    public interface IPad
    {
        public EGamePadState PadState { get; }

        void EnableGamepad();
        void DisableGamepad();
    }

    public class Pad : MonoBehaviour, IPad
    {
        /// ------------------
        /// Const, Readonly
        /// ------------------
        private const string    LAYER_REFLECTION_WALL   = "ReflectionWall";
        private const int       MAXIMUM_LINE_COUNT      = 3;

        private const float     MIN_Y = -4.5f;

        public EGamePadState PadState => _padState;

        /// ------------------
        /// SerializedField
        /// ------------------        
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private LayerMask    _layerMask;



        /// ------------------
        /// Private
        /// ------------------
        private EGamePadState _padState = EGamePadState.Disabled;
        
        private Vector2 _shootBall_TF;
        private Camera  _cam;

        private Vector2 _startPos;
        private Vector2 _mousePos;
        
        // private Vector2 _hitGridPoint;

        private Action<Vector2> _shootAction;

        // public Vector2 HitGridPoint => _hitGridPoint;

        public void Init(Vector3 shootBall_TR, Action<Vector2> shootAction)
        {
            _shootBall_TF   = shootBall_TR;
            _cam            = Camera.main;
            _shootAction    = shootAction;
            _startPos       = _shootBall_TF;

            _padState       = EGamePadState.Disabled;

            _lineRenderer.positionCount = MAXIMUM_LINE_COUNT;
            _lineRenderer.gameObject.SetActive(false);
        }   

        private void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if(_padState == EGamePadState.Disabled)
            {
                return;
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _lineRenderer.gameObject.SetActive(true);
                }
                else if (Input.GetMouseButton(0))
                {
                    _mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);

                    RaycastHit2D hit = Physics2D.Raycast(_startPos, _mousePos - _startPos, Mathf.Infinity, _layerMask);

                    if (hit.collider != null)
                    {
                        _lineRenderer.SetPosition(0, _shootBall_TF);
                        _lineRenderer.SetPosition(1, hit.point);
                        _lineRenderer.SetPosition(2, hit.point);

                        if (hit.collider.gameObject.layer == LayerMask.NameToLayer(LAYER_REFLECTION_WALL))
                            ReflectionDrawLine(hit);
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    _lineRenderer.gameObject.SetActive(false);

                    if (_mousePos.y < MIN_Y)
                        return;

                    _shootAction?.Invoke(_mousePos);
                    _padState = EGamePadState.Disabled;
                }
            }
        }

        private void ReflectionDrawLine(RaycastHit2D hit)
        {
            RaycastHit2D hit2D;
            Vector2 inDirection     = (hit.point - _startPos).normalized;
            Vector2 reflectionDir   = Vector2.Reflect(inDirection, hit.normal);

            hit2D = Physics2D.Raycast(hit.point + (reflectionDir * 0.001f), reflectionDir, Mathf.Infinity, _layerMask);

            if(hit2D.collider == null)
                return;

            // _hitGridPoint = hit2D.point;
            _lineRenderer.SetPosition(2, hit2D.point);
        }


        /// ------------
        /// Interface
        /// ------------
        public void EnableGamepad()
        {
            _padState = EGamePadState.InUse;
        }

        public void DisableGamepad()
        {
            _padState = EGamePadState.Disabled;
        }
    }
}