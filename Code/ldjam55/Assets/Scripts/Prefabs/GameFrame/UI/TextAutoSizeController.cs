using System;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

namespace Assets.Scripts
{
    public enum TextResizePattern
    {
        IgnoreRichText,
        AllCharacters,
    }

    [ExecuteAlways]
    public class TextAutoSizeController : MonoBehaviour
    {
        [SerializeField] private List<TMP_Text> _labels = new List<TMP_Text>();
        [SerializeField] private TextResizePattern _pattern;
        [SerializeField] private bool _executeOnUpdate;
        [SerializeField] private bool _searchChildren = false;
        private int _currentIndex;

        private void Start()
        {
            if (_searchChildren)
            {
                var tt = transform.GetComponentsInChildren<TMP_Text>(true);
                _labels = new List<TMP_Text>(tt);
                Execute();
            }
        }

        public void ForceChildren()
        {
            var tt = transform.GetComponentsInChildren<TMP_Text>(true);
            _labels = new List<TMP_Text>(tt);
            Execute();
        }

        public void AddLabel(TMP_Text label)
        {
            this._labels.Add(label);
        }

        public void Execute()
        {
            if (_labels.Count == 0)
            {
                return;
            }

            int count = _labels.Count;

            int index = 0;
            float maxLength = 0;

            for (int i = 0; i < count; i++)
            {
                float length = 0;

                switch (_pattern)
                {
                    case TextResizePattern.IgnoreRichText:
                        length = _labels[i].GetParsedText().Length;
                        break;

                    case TextResizePattern.AllCharacters:
                        length = _labels[i].text.Length;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (length > maxLength)
                {
                    maxLength = length;
                    index = i;
                }
            }

            if (_currentIndex != index)
            {
                OnChanged(index);
            }
        }

        private void OnChanged(int index)
        {
            // Disable auto size on previous
            _labels[_currentIndex].enableAutoSizing = false;

            _currentIndex = index;

            // Force an update of the candidate text object so we can retrieve its optimum point size.
            _labels[index].enableAutoSizing = true;
            _labels[index].ForceMeshUpdate();
        }

        private void OnUpdateCheck()
        {
            // Iterate over all other text objects to set the point size
            int count = _labels.Count;

            if (count > 0)
            {
                float optimumPointSize = _labels[_currentIndex].fontSize;

                for (int i = 0; i < count; i++)
                {
                    if (_currentIndex != i)
                    {
                        _labels[i].enableAutoSizing = false;
                        _labels[i].fontSize = optimumPointSize;
                    }
                }
            }
        }

        private void Update()
        {
            if (_executeOnUpdate)
            {
                Execute();
            }

            OnUpdateCheck();
        }
    }
}
