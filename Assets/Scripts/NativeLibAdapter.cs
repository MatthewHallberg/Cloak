using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class NativeLibAdapter : MonoBehaviour {

#if UNITY_EDITOR
    [DllImport("macPlugin")]
    private static extern int RecieveImage(byte[] bytes, int width, int height, bool isGreen);
    [DllImport("macPlugin")]
    private static extern void SaveBackground();
    [DllImport("macPlugin")]
    private static extern void SetViewTextureFromUnity(IntPtr texture, int w, int h);
    [DllImport("macPlugin")]
    private static extern IntPtr GetRenderEventFunc();
#elif PLATFORM_IOS
    [DllImport("__Internal")]
    private static extern void RecieveImage(byte[] bytes, int width, int height, bool isGreen);
    [DllImport("__Internal")]
    private static extern void SaveBackground();
    [DllImport("__Internal")]
    private static extern void SetViewTextureFromUnity(IntPtr texture, int w, int h);
    [DllImport("__Internal")]
    private static extern IntPtr GetRenderEventFunc();
#else
    [DllImport("OpenCVPlugin")]
    private static extern int RecieveImage(byte[] bytes, int width, int height, bool isGreen);
    [DllImport("OpenCVPlugin")]
    private static extern void SaveBackground();
    [DllImport("OpenCVPlugin")]
    private static extern void SetViewTextureFromUnity(IntPtr texture, int w, int h);
    [DllImport("OpenCVPlugin")]
    private static extern IntPtr GetRenderEventFunc();
#endif

    public void PassViewTextureToPlugin(Texture2D tex) {
        // Pass texture pointer to the plugin
        SetViewTextureFromUnity(tex.GetNativeTexturePtr(), tex.width, tex.height);
    }

    public void StartOnRenderEvent() {
        StartCoroutine(CallPluginAtEndOfFrames());
    }

    IEnumerator CallPluginAtEndOfFrames() {
        yield return new WaitForSeconds(1f);
        while (true) {
            // Wait until all frame rendering is done
            yield return new WaitForEndOfFrame();
            GL.IssuePluginEvent(GetRenderEventFunc(), 1);
            GC.Collect();
        }
    }

    public void SaveSnapshot() {
        SaveBackground();
        Debug.Log("Saving snapshot...");
    }

    public void SendImage(Texture2D tex) {
       bool isGreen = true;
       RecieveImage(tex.GetRawTextureData(), tex.width, tex.height, isGreen);
    }
}