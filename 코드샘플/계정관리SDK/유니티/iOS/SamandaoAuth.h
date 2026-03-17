//
//  SamandaoAuth.h
//  Unity-iPhone
//
//  Created by Sandbox Gamestudio on 2021/02/09.
//

#ifndef SamandaoAuth_h
#define SamandaoAuth_h

#import <Foundation/Foundation.h>
#import <GoogleSignIn/GoogleSignIn.h>

@interface SamandaoAuth : NSObject<GIDSignInDelegate>

+ (SamandaoAuth *)sharedInstance;

+ (void)onSamandaStart;
+ (void)startoAuthApple;
+ (void)startoAuthGoogle;
+ (void)responseoAuthWithDelegate: (int)error idToken:(NSString *)token;

@end

#ifdef __cplusplus
extern "C" {
#endif

void SamandaNativeInit();

typedef void (*SamandaoAuthCallbackType)(int error, const char* token);
void SamandaNativeoAuthApple(SamandaoAuthCallbackType callback);
void SamandaNativeoAuthGoogle(SamandaoAuthCallbackType callback);

#ifdef __cplusplus
} // extern "C" {
#endif

#endif /* SamandaoAuth_h */
