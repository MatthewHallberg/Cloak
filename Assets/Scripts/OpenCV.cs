using UnityEngine;
using UnityEngine.UI;

public class OpenCV : MonoBehaviour {

    [HideInInspector]
    public TextureFormat sendFormat = TextureFormat.RGB24;
    [HideInInspector]
    public TextureFormat viewFormat = TextureFormat.RGBA32;

    [SerializeField]
    public NativeLibAdapter nativeLibAdapter;

    [Header("Camera Feeds")]
    public ARCamFeed arCamFeed;
    public EditorCamFeed editorCamFeed;

    [Header("UI")]
    public RawImage screenImageFromPlugin;
    public AspectRatioFitter ratioFitter;

    Texture2D writableTexture;

    void Start() {
        Application.targetFrameRate = 30;
        //initialize camera feed
#if UNITY_EDITOR
        editorCamFeed.Init();
#else
        arCamFeed.Init();
#endif
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            nativeLibAdapter.SaveSnapshot();
        }
    }

    public void CreateWritableTexture(int width, int height) {

        Debug.Log(width + " : " + height);

        //set UI cam image aspect ratio
        float aspectRatio = (float)width / (float)height;
        ratioFitter.aspectRatio = aspectRatio;

        //set up cam textures
        writableTexture = new Texture2D(width, height, viewFormat, false);
        
        nativeLibAdapter.PassViewTextureToPlugin(writableTexture);
        screenImageFromPlugin.texture = writableTexture;

        //start rendering event
        nativeLibAdapter.StartOnRenderEvent();
    }

    public void ProcessImage(Texture2D texture) {
        nativeLibAdapter.SendImage(texture);
    }
}