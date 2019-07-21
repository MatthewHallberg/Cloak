#ifndef OpenCVPlugin_hpp
#define OpenCVPlugin_hpp

#include <stdio.h>

__declspec(dllexport) void SaveBackground();
__declspec(dllexport) void RecieveImage(unsigned char* bytes, int width, int height, bool isGreen);

unsigned char* GetCurrImage();

#endif /* OpenCVPlugin_hpp */


