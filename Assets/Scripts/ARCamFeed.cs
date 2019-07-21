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

    OpenCV openCV;
    Texture2D textureToSend;
    bool texturesCreated;

    public void Init() {
        openCV = GetComponent<OpenCV>();
        arCameraManager.frameReceived += OnCameraFrameReceived;
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

        XRCameraImageConversionParams conversionParams = new XRCameraImageConversionParams {
            // Get the entire image
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample
            outputDimensions = new Vector2Int(image.width / 3, image.height / 3),

            // Choose RGB format
            outputFormat = openCV.sendFormat,

            // Flip across the vertical axis (mirror image)
            transformation = CameraImageTransformation.MirrorX
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