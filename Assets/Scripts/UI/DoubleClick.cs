using UnityEngine;
using UnityEngine.EventSystems;

public class DoubleClick : MonoBehaviour {
    protected void OnPointerDown(PointerEventData eventData) {
        if (eventData.clickCount == 2) {
            OnDoubleClickCallback();
        }
    }

    protected virtual void OnDoubleClickCallback() { }
}
