using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PurchaseSlot : MonoBehaviour, IPointerClickHandler {

    public List<GameObject> _gfxList;
    public string _layerName;
    public Transform rtParent;
    public Text txt_CostValue;
    public Text txt_ChessName;

    private GameObject slotGfx;
    private float lastClick = 0f;
    private float interval = .2f;
    private bool allowedToPurchase = false;
    private ChessProp _chessProp;

    private void Start() {
        SlotRefresh();
    }
    
    // 刷新
    public void SlotRefresh() {
        DisplayChessRT();
        DisplayChessInfo();
    }
    
    // 打开
    public void OpenPurchasePanel() {

    }

    // 关闭
    public void ClosePurchasePanel() {

    }

    private void DisplayChessRT() {
        var count = _gfxList.Count;
        var _gfxPrefab = _gfxList[Random.Range(0, count)];

        var _gfx = Instantiate(_gfxPrefab, rtParent);

        slotGfx = _gfx;

        _gfx.transform.localPosition = Vector3.zero;
        _gfx.transform.localRotation = Quaternion.identity;

        ChangeLayersRecursively(rtParent, _layerName);
        allowedToPurchase = true;
    }

    private void DisplayChessInfo() {
        _chessProp = slotGfx.GetComponent<Character>()._chessProp;

        if (txt_CostValue != null) {
            txt_CostValue.text = $"×{_chessProp.cost.GetValue}";
        }

        if (txt_ChessName != null) {
            txt_ChessName.text = $"{_chessProp.chessName}";
        }
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
            if (!GameManager.Instance.PurchaseChessToBackup(_chessProp)) {
                return;
            }

            if (slotGfx != null) {
                slotGfx.SetActive(false);
            }

            allowedToPurchase = false;
        } else {
            // signle click
            lastClick = Time.time;
        }
    }
}

