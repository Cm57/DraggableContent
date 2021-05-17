using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DraggableContent
{
    [RequireComponent(typeof(HorizontalOrVerticalLayoutGroup))]
    public class DragDropContent : MonoBehaviour
    {
        public event Action<Transform, int, int> OnDragDone;
        private LayoutElement _emptyLayoutElement;

        private float _topOrLeftPadding;
        private float _spacing;
        private bool _isHorizontal;

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.GetComponent<LayoutElement>() is null)
                {
                    child.gameObject.AddComponent<LayoutElement>();
                }

                if (child.GetComponent<DragElement>() is null)
                {
                    child.gameObject.AddComponent<DragElement>();
                }
            }
            
            if (_emptyLayoutElement is null)
            {
                _emptyLayoutElement = new GameObject("EmptyPlaceHolder").AddComponent<LayoutElement>();
                _emptyLayoutElement.ignoreLayout = true;
                _emptyLayoutElement.transform.SetParent(transform, false);
            }
            else
            {
                _emptyLayoutElement.transform.SetAsLastSibling();
            }
        }

        private void OnEnable()
        {
            var layoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
            _isHorizontal = layoutGroup is HorizontalLayoutGroup;
            _topOrLeftPadding = _isHorizontal ? layoutGroup.padding.left : layoutGroup.padding.top;
            var rectTransform = (RectTransform) transform;
            var opVector = _isHorizontal ? Vector2.right : Vector2.up;
            var pivot = rectTransform.pivot * (Vector2.one - opVector) + opVector * Vector2.up;
            var pos = rectTransform.anchoredPosition;
            pos.x -= rectTransform.rect.width * (rectTransform.pivot.x - pivot.x);
            pos.y -= rectTransform.rect.height * (rectTransform.pivot.y - pivot.y);
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = pos;
            _spacing = layoutGroup.spacing;
        }

        public void SetupPlaceHolder(Transform tr)
        {
            var rtRect = ((RectTransform) tr).rect;
            var placeHolderRt = (RectTransform) _emptyLayoutElement.transform;
            placeHolderRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rtRect.width);
            placeHolderRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rtRect.height);
            _emptyLayoutElement.preferredHeight = rtRect.height;
            _emptyLayoutElement.preferredWidth = rtRect.width;
            _emptyLayoutElement.ignoreLayout = false;
        }

        private void ApplyPlaceHolder(int index)
        {
            _emptyLayoutElement.transform.SetSiblingIndex(index);
        }

        public void CancelPlaceHolder()
        {
            _emptyLayoutElement.ignoreLayout = true;
            _emptyLayoutElement.transform.SetAsLastSibling();
        }

        public void OnDragDoneCallback(Transform tr, int startSibling, int endSibling)
        {
            OnDragDone?.Invoke(tr, startSibling, endSibling);
        }

        public int OnElementDrag(PointerEventData eventData, Transform elementTransform)
        {
            var position = elementTransform.position;
            if (_isHorizontal)
                position.x = eventData.position.x;
            else
                position.y = eventData.position.y;
            elementTransform.position = position;
            var sibling = GetDragSibling(elementTransform);
            ApplyPlaceHolder(sibling);
            return sibling;
        }

        private int GetDragSibling(Transform tr)
        {
            var rect = ((RectTransform) tr).rect;
            var sizeDelta = _isHorizontal ? rect.width : rect.height;
            var trLocalPosition = tr.localPosition;
            var startOffset = _isHorizontal ? trLocalPosition.x : -trLocalPosition.y;
            return (int) ((startOffset - _topOrLeftPadding) / (sizeDelta + _spacing));
        }
    }
}