using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BubbleWitchSaga.Stage
{
    public interface IView
    {
        public void Init(Action[] actions);
        void ShowStartView();
        void ShowEndView();
    }

    public enum EViewType
    {
        Start,
        End
    }

    public class StageView : MonoBehaviour, IView
    {
        [System.Serializable]
        private class View
        {
            public EViewType    ViewType;
            public GameObject   Parent;
            public Button       Button;

            private Action      _buttonAction;

            public void Show()
            {
                Parent.SetActive(true);
            }

            public void Hide()
            {
                Parent.SetActive(false);
            }

            public void AddListnerButton(Action action)
            {
                Button.onClick.RemoveAllListeners();

                _buttonAction = action;
                _buttonAction += Hide;

                Button.onClick.AddListener(()=> _buttonAction?.Invoke());
            }
        }

        [SerializeField] private View[] _views;
        [SerializeField] private CanvasGroup _canvasGroup;


        private Dictionary<EViewType, View> _viewDict;

        public void Init(Action[] actions)
        {
            _viewDict = new Dictionary<EViewType, View>();
            for (int i = 0; i < _views.Length; i++)
            {
                View view = _views[i];

                if(_viewDict.ContainsKey(view.ViewType))
                    continue;

                _viewDict.Add(view.ViewType, view);

                view.AddListnerButton(() => SetupCnavas(0, false, false));
                view.AddListnerButton(actions[i]);
                view.Hide();
            }

            DontDestroyOnLoad(this);
        }

        private void SetupCnavas(int alpha, bool blocksRaycasts, bool interactable)
        {
            _canvasGroup.alpha = alpha;
            _canvasGroup.blocksRaycasts = blocksRaycasts;
            _canvasGroup.interactable = interactable;
        }

        public void ShowEndView()
        {
            SetupCnavas(1, true, true);
            _viewDict[EViewType.End].Show();
        }

        public void ShowStartView()
        {
            SetupCnavas(1, true, true);
            _viewDict[EViewType.Start].Show();
        }
    }
}