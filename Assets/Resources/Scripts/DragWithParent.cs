using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragWithParent : MonoBehaviour, IPointerDownHandler, IDragHandler
{
        private Vector3 pointerPos;
        private Vector3 parentPos;

        public void OnPointerDown(PointerEventData eventData)
        {
                pointerPos = Camera.main.ScreenToWorldPoint(eventData.position);
                parentPos = transform.parent.position;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
                Vector3 offset = Camera.main.ScreenToWorldPoint(eventData.position) - pointerPos;
                transform.parent.position = new Vector3(parentPos.x + offset.x, parentPos.y + offset.y, parentPos.z);
        }
}