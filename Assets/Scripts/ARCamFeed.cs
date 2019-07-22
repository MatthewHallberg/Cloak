using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(OpenCV))]
public class ARCamFeed : MonoBehaviour {

    [SerializeField]
    public ARCameraManager arCameraManager;

    [SerializeField]
    Transform camImageScreen;

    OpenCV openCV;
    Texture2D textureToSend;
    bool texturesCreated;

    public void Init() {
        openCV = GetComponent<OpenCV>();
        arCameraManager.frameReceived += OnCameraFrameReceived;
        camImageScreen.localScale = Vector3.one;
    }

    void OnDisable() {
        if (texturesCreated) {
            arCameraManager.frameReceived -= OnCameraFrameReceived;
        }
    }

    unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs) {

        if (!arCameraManager.TryGetLatestImage(out XRCameraImage image)) {
            return;
        }

        CameraImageTransformation camTransform = CameraImageTransformation.None;

        if (Screen.orientation == ScreenOrientation.Portrait) {
            //mirror x and turn 180
            camTransform = CameraImageTransformation.None;
            camImageScreen.localEulerAngles = new Vector3(0, 0, 180);
        } else {
            //assuming landscape left
            camTransform = CameraImageTransformation.MirrorX;
            camImageScreen.localEulerAngles = Vector3.zero;
        }


        XRCameraImageConversionParams conversionParams = new XRCameraImageConversionParams {
            // Get the entire image
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample
            outputDimensions = new Vector2Int(image.width / 3, image.height / 3),

            // Choose RGB format
            outputFormat = openCV.sendFormat,

            transformation = camTransform
        };

        // See how many bytes we need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

        image.Dispose();

        if (textureToSend == null) {
            textureToSend = new Texture2D(
                conversionParams.outputDimensions.x,
                conversionParams.outputDimensions.y,
                conversionParams.outputFormat,
            false);
        }

        textureToSend.LoadRawTextureData(buffer);
        textureToSend.Apply();

        if (!texturesCreated) {
            texturesCreated = true;
            //init textures here
            openCV.CreateWritableTexture(textureToSend.width, textureToSend.height);
            return;
        }

        //process the image
        openCV.ProcessImage(textureToSend);

        // Done with our temporary data
        buffer.Dispose();
    }
}