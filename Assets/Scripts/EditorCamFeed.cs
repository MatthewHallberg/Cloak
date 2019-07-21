using UnityEngine;

[RequireComponent(typeof(OpenCV))]
public class EditorCamFeed : MonoBehaviour {

    OpenCV openCV;
    WebCamTexture webCamTex;
    Texture2D textureToSend;

    public void Init() {
        openCV = GetComponent<OpenCV>();
        webCamTex = new WebCamTexture(WebCamTexture.devices[0].name, 640, 480, 30);
        webCamTex.Play();
    }

#if UNITY_EDITOR
    private void Update() {
        if (webCamTex.width > 100) {
            if (textureToSend == null) {
                openCV.CreateWritableTexture(webCamTex.width, webCamTex.height);
                textureToSend = new Texture2D(webCamTex.width, webCamTex.height, openCV.sendFormat, false);
                return;
            }
            textureToSend.SetPixels32(webCamTex.GetPixels32());
            textureToSend.Apply();
            openCV.ProcessImage(textureToSend);
        }
    }
#endif
}
