using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class ImageCreator : MonoBehaviour {

    public OpenCV openCV;

    public Texture2D customImage;

    VideoPlayer videoPlayer;
    Texture2D tempTexture;

    private void Awake() {
        videoPlayer = GetComponent<VideoPlayer>();
        tempTexture = new Texture2D(2, 2, openCV.sendFormat, false);
    }

    void Update() {
        if (videoPlayer.isPlaying) {
            UpdateVideoFrame();
        }
    }

    public void SendVideoBackground() {
        if (!videoPlayer.isPlaying) {
            videoPlayer.Play();
        }
    }

    public void SendCurrentBackground() {
        videoPlayer.Stop();
        openCV.SetBackgroundImage(openCV.currentCamImage, false, false);
    }

    public void SendCustomBackground() {
        videoPlayer.Stop();

        tempTexture.Resize(customImage.width, customImage.height);
        tempTexture.SetPixels32(customImage.GetPixels32());
        tempTexture.Apply();
#if UNITY_EDITOR
        openCV.SetBackgroundImage(tempTexture, true, false);
#else
        openCV.SetBackgroundImage(tempTexture, false, true);
#endif
    }

    void UpdateVideoFrame() {
        RenderTexture renderTexture = videoPlayer.texture as RenderTexture;
        if (tempTexture.width != renderTexture.width || tempTexture.height != renderTexture.height) {
            tempTexture.Resize(renderTexture.width, renderTexture.height);
        }
        RenderTexture.active = renderTexture;
        tempTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tempTexture.Apply();
        RenderTexture.active = null;
#if UNITY_EDITOR
        openCV.SetBackgroundImage(tempTexture, true, false);
#else
        openCV.SetBackgroundImage(tempTexture, false, true);
#endif
    }
}
