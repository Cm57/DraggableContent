using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DraggableContent
{
    [RequireComponent(typeof(LayoutElement))]
    public class DragElement : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        private LayoutElement _layoutElement;
        private DragDropContent _dragDropContent;
        private int _startSibling, _endSibling;

        private void Awake()
        {
            _layoutElement = GetComponent<LayoutElement>();
            _dragDropContent = transform.parent.GetComponent<DragDropContent>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startSibling = transform.GetSiblingIndex();
            transform.SetAsLastSibling();
            _dragDropContent.SetupPlaceHolder(transform);
            _layoutElement.ignoreLayout = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _endSibling = _dragDropContent.OnElementDrag(eventData, transform);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _dragDropContent.CancelPlaceHolder();
            transform.SetSiblingIndex(_endSibling);
            _layoutElement.ignoreLayout = false;
            _dragDropContent.OnDragDoneCallback(transform, _startSibling, _endSibling);
        }
    }
}