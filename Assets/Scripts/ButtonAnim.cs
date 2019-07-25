using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class ButtonAnim : MonoBehaviour {

    const float SPEED = 6f;

    public UnityEvent onComplete;
    
    Vector3 desiredScale;
    Vector3 startScale;
    Vector3 endScale;

    bool onCompleteCalled = true;
    
    public virtual void Start() {
        startScale = transform.localScale;
        endScale = startScale * 1.5f;
        desiredScale = startScale;
    }

    // Update is called once per frame
    public virtual void Update() {
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * SPEED);
        if (desiredScale == endScale && Vector3.Distance(transform.localScale,endScale) < .1f) {
            desiredScale = startScale;
        }

        if (desiredScale == startScale && Vector3.Distance(transform.localScale, startScale) < .2f) {
            if (!onCompleteCalled) {
                onCompleteCalled = true;
                onComplete?.Invoke();
            }
        }
    }

    public void SetSize(Vector3 scale) {
        startScale = scale;
        endScale = scale * 1.5f;
        desiredScale = endScale;
    }

    public void ButtonAnimation() {
        onCompleteCalled = false;
        GetComponent<AudioSource>().Play();
        desiredScale = endScale;
    }
}
