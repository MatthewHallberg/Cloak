// Example low level rendering Unity plugin

#include "PlatformBase.h"
#include "RenderAPI.h"

#include <stdio.h>
#include <iostream>
#include <fstream>
#include <stdio.h>
#include <assert.h>
#include <math.h>
#include <vector>

using namespace std;

// --------------------------------------------------------------------------
// SetTextureFromUnity, an example function we export which is called by one of the scripts.

static void* g_ViewTextureHandle = NULL;
static int   g_ViewTextureWidth  = 0;
static int   g_ViewTextureHeight = 0;

unsigned char* GetCurrImage();

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetViewTextureFromUnity(void* textureHandle, int w, int h) {
    // A script calls this at initialization time; just remember the texture pointer here.
    // Will update texture pixels each frame from the plugin rendering event (texture update
    // needs to happen on the rendering thread).
    g_ViewTextureHandle = textureHandle;
    g_ViewTextureWidth = w;
    g_ViewTextureHeight = h;
}

// --------------------------------------------------------------------------
// UnitySetInterfaces

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;

extern "C" void    UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_UnityInterfaces = unityInterfaces;
    s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
    s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
    
#if SUPPORT_VULKAN
    if (s_Graphics->GetRenderer() == kUnityGfxRendererNull)
    {
        extern void RenderAPI_Vulkan_OnPluginLoad(IUnityInterfaces*);
        RenderAPI_Vulkan_OnPluginLoad(unityInterfaces);
    }
#endif // SUPPORT_VULKAN
    
    // Run OnGraphicsDeviceEvent(initialize) manually on plugin load
    OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

#if UNITY_WEBGL
typedef void    (UNITY_INTERFACE_API * PluginLoadFunc)(IUnityInterfaces* unityInterfaces);
typedef void    (UNITY_INTERFACE_API * PluginUnloadFunc)();

extern "C" void    UnityRegisterRenderingPlugin(PluginLoadFunc loadPlugin, PluginUnloadFunc unloadPlugin);

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API RegisterPlugin()
{
    UnityRegisterRenderingPlugin(UnityPluginLoad, UnityPluginUnload);
}
#endif

// --------------------------------------------------------------------------
// GraphicsDeviceEvent


static RenderAPI* s_CurrentAPI = NULL;
static UnityGfxRenderer s_DeviceType = kUnityGfxRendererNull;


static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
    // Create graphics API implementation upon initialization
    if (eventType == kUnityGfxDeviceEventInitialize)
    {
        assert(s_CurrentAPI == NULL);
        s_DeviceType = s_Graphics->GetRenderer();
        s_CurrentAPI = CreateRenderAPI(s_DeviceType);
    }
    
    // Let the implementation process the device related events
    if (s_CurrentAPI)
    {
        s_CurrentAPI->ProcessDeviceEvent(eventType, s_UnityInterfaces);
    }
    
    // Cleanup graphics API implementation upon shutdown
    if (eventType == kUnityGfxDeviceEventShutdown)
    {
        delete s_CurrentAPI;
        s_CurrentAPI = NULL;
        s_DeviceType = kUnityGfxRendererNull;
    }
}

// --------------------------------------------------------------------------
// OnRenderEvent
// This will be called for GL.IssuePluginEvent script calls; eventID will
// be the integer passed to IssuePluginEvent. In this example, we just ignore
// that value.
static void ModifyTexturePixels() {
    void* textureHandle = g_ViewTextureHandle;
    int width = g_ViewTextureWidth;
    int height = g_ViewTextureHeight;
    if (!textureHandle)
        return;
    
    unsigned char* textureDataPtr = GetCurrImage();
    
    s_CurrentAPI->EndModifyTexture(textureHandle, width, height, width * 4, textureDataPtr);
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
    // Unknown / unsupported graphics device type? Do nothing
    if (s_CurrentAPI == NULL)
        return;
    
    ModifyTexturePixels();
}


// --------------------------------------------------------------------------
// GetRenderEventFunc, an example function we export which is used to get a rendering event callback function.

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc() {
    return OnRenderEvent;
}




