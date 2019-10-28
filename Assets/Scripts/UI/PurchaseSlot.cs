using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PurchaseSlot : MonoBehaviour, IPointerClickHandler {

    public List<GameObject> _gfxList;
    public string _layerName;
    public Transform rtParent;

    private GameObject slotGfx;
    private float lastClick = 0f;
    private float interval = .2f;
    private bool allowedToPurchase = false;

    private void Start() {
        SlotRefresh();
    }
    
    public void SlotRefresh() {
        var count = _gfxList.Count;
        var _gfxPrefab = _gfxList[Random.Range(0, count)];

        var _gfx = Instantiate(_gfxPrefab, rtParent);
        slotGfx = _gfx;

        _gfx.transform.localPosition = Vector3.zero;
        _gfx.transform.localRotation = Quaternion.identity;

        ChangeLayersRecursively(rtParent, _layerName);
        allowedToPurchase = true;
    }

    private void ChangeLayersRecursively(Transform trans, string name) {
        trans.gameObject.layer = LayerMask.NameToLayer(name);

        foreach (Transform child in trans) {
            ChangeLayersRecursively(child, name);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData) {
        if (!allowedToPurchase) {
            return;
        }

        // double click
        if ((lastClick + interval) > Time.time) {
            //Debug.Log($"double click {transform.name}");
            var character = slotGfx.GetComponent<Character>();
            GameManager.Instance.PurchaseChessToBackup(character.characterType);

            if (slotGfx != null)
                slotGfx.SetActive(false);

            allowedToPurchase = false;
        } else {
            // signle click
            lastClick = Time.time;
        }
    }
}

