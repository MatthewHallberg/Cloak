#import "UnityAppController.h"

extern "C" void UnityPluginLoad(IUnityInterfaces *interfaces);
extern "C" void UnityPluginUnload();

@interface PluginAppController : UnityAppController
{
}
- (void)shouldAttachRenderDelegate;
@end

@implementation PluginAppController
- (void)shouldAttachRenderDelegate;
{
    UnityRegisterRenderingPluginV5(&UnityPluginLoad, &UnityPluginUnload);
}
@end

IMPL_APP_CONTROLLER_SUBCLASS(PluginAppController);

