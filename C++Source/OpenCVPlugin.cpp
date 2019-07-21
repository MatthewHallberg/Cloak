#include <stdio.h>
#include <opencv2/opencv.hpp>

using namespace std;
using namespace cv;

Mat frame;
Mat background;
Mat final_output;
Mat unity_tex;

unsigned char* GetCurrImage(){
    if (unity_tex.cols > 1){
        cvtColor(unity_tex, unity_tex, COLOR_RGB2RGBA);
        return unity_tex.data;
    }
    cvtColor(frame, frame, COLOR_RGB2RGBA);
    return frame.data;
}

extern "C" {
    
    void SaveBackground(){
        frame.copyTo(background);
    }
    
    void RecieveImage(unsigned char* bytes, int width, int height, bool isGreen){
        
        //process incoming stream before we can use it
        frame = Mat(height, width, CV_8UC3, static_cast<void*>(bytes));
        
        if (!frame.empty()){
            
            if (background.cols > 1){
                
                //Converting image from BGR to HSV color space.
                Mat hsv;
                cvtColor(frame, hsv, COLOR_RGB2HSV);
                
                Mat mask1,mask2;
                // Creating masks to detect the upper and lower bounds of color
                
                if (isGreen){
                    //green
                    inRange(hsv, Scalar(35, 40, 20), Scalar(100, 255, 255), mask1);
                    inRange(hsv, Scalar(290, 100, 70), Scalar(300, 100, 100), mask2);
                } else {
                    //red
                    inRange(hsv, Scalar(0, 180, 70), Scalar(10, 255, 255), mask1);
                    inRange(hsv, Scalar(170, 120, 70), Scalar(180, 255, 255), mask2);
                }
                
                // Generating the final mask
                mask1 = mask1 + mask2;
                
                Mat kernel = Mat::ones(3,3, CV_32F);
                morphologyEx(mask1,mask1,cv::MORPH_OPEN,kernel);
                morphologyEx(mask1,mask1,cv::MORPH_DILATE,kernel);
                
                // creating an inverted mask to segment out the cloth from the frame
                bitwise_not(mask1,mask2);
                Mat res1, res2, final_output;
                
                // Segmenting the cloth out of the frame using bitwise and with the inverted mask
                bitwise_and(frame,frame,res1,mask2);
                
                // creating image showing static background frame pixels only for the masked region
                bitwise_and(background,background,res2,mask1);
                
                // Generating the final augmented output.
                addWeighted(res1,1,res2,1,0,final_output);
                final_output.copyTo(unity_tex);
            }
        }
    }
}

