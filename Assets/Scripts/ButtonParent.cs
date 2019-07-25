using System.Collections.Generic;
using UnityEngine;

public class ButtonParent : MonoBehaviour {

    const float SPEED = 6f;

    Vector2 desiredScale = Vector2.zero;

    List<RectTransform> children = new List<RectTransform>();

    private void Awake() {
        foreach (Transform child in transform) {
            children.Add(child.GetComponent<RectTransform>());
        }
    }

    void Update() {

        if (Input.GetMouseButtonDown(0) && desiredScale == Vector2.zero) {
            OpenMenu();
        }
        foreach (RectTransform child in children) {
            child.sizeDelta = Vector2.Lerp(child.sizeDelta, desiredScale, Time.deltaTime * SPEED);
        }
    }

    public void OpenMenu() {
        desiredScale = new Vector2(125, 125);
    }

    public void CloseMenu() {
        desiredScale = Vector2.zero;
    }
}
