//
//  SamandaoAuth.mm
//  UnityFramework
//
//  Created by Sandbox Gamestudio on 2021/02/09.
//

#import "UnityAppController.h"
#import "SamandaoAuth.h"

SamandaoAuthCallbackType oAuthCallback = NULL;

@implementation SamandaoAuth

+ (SamandaoAuth *)sharedInstance
{
    static SamandaoAuth *sharedClass = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedClass = [[self alloc] init];
    });
    return sharedClass;
}

- (id)init
{
    self = [super init];
    return self;
}

+ (void)onSamandaStart
{
    NSLog(@"%s", "onSamandaStart");
        
    [GIDSignIn sharedInstance].clientID = @"";
    [GIDSignIn sharedInstance].delegate = [SamandaoAuth sharedInstance];
}

+ (void)startoAuthApple
{
    NSLog(@"%s", "startoAuthApple");
    
    // test
    [self responseoAuthWithDelegate:0 idToken:[NSString stringWithUTF8String:"tokenbody"]];
}

+ (void)startoAuthGoogle
{
    [[GIDSignIn sharedInstance] signIn];
}

- (void)signIn:(GIDSignIn *)signIn didSignInForUser:(GIDGoogleUser *)user withError:(NSError *)error {
    if (NULL == error)
    {
        [SamandaoAuth responseoAuthWithDelegate:0 idToken:user.authentication.idToken];
    }
    else
    {
        [SamandaoAuth responseoAuthWithDelegate:(int)error.code idToken:@""];
    }
}

+ (void)responseoAuthWithDelegate: (int)error idToken:(NSString *)token
{
    //NSLog(@"%s", "responseoAuthApple");
    
    if (NULL != oAuthCallback)
    {
        oAuthCallback(error, [token cStringUsingEncoding:NSUTF8StringEncoding]);
    }
}

@end

// extern "C" functions
void SamandaNativeInit()
{
    [SamandaoAuth onSamandaStart];
}

void SamandaNativeoAuthApple(SamandaoAuthCallbackType callback)
{
    oAuthCallback = callback;
    
    [SamandaoAuth startoAuthApple];
}

void SamandaNativeoAuthGoogle(SamandaoAuthCallbackType callback)
{
    oAuthCallback = callback;
    
    [GIDSignIn sharedInstance].presentingViewController = UnityGetGLViewController();
    
    [SamandaoAuth startoAuthGoogle];
}

// EOF
