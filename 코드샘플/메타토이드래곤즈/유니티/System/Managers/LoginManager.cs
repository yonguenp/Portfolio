using UnityEngine;

using Firebase.Auth;
using System;

#if UNITY_ANDROID && !UNITY_EDITOR



#elif UNITY_IOS && !UNITY_EDITOR

using AppleAuth;
using AppleAuth.Native;

#endif



namespace SandboxNetwork
{
    public class LoginManager
    {
        static LoginManager instance = null;
        public static LoginManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LoginManager();
                }
                return instance;
            }
        }

        // FireBase
        FirebaseAuth auth;
        public FirebaseAuth Auth { get { return auth; } }

        public bool isGoogleLoginInitialized = false;
        public bool isAppleLoginInitialized = false;

#if UNITY_ANDROID && !UNITY_EDITOR

#elif UNITY_IOS && !UNITY_EDITOR

        IAppleAuthManager appleAuthManager;
        public IAppleAuthManager AppleAuthManager_Instance { get { return appleAuthManager; } }

#endif
        public void InitializeLogin()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;

                    isGoogleLoginInitialized = true;

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                    FirebaseLoadDone();
                }
                else
                {
                    isGoogleLoginInitialized = false;

                    try
                    {
                        FirebaseLoadDone();
                    }
                    catch {
                        Debug.LogError(string.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    }
                }
            });
#elif UNITY_IOS && !UNITY_EDITOR

            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
                var deserializer = new PayloadDeserializer();
                // Creates an Apple Authentication manager with the deserializer
                appleAuthManager = new AppleAuthManager(deserializer);
            }

            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                    isAppleLoginInitialized = true;

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                    FirebaseLoadDone();
                }
                else
                {
                    isAppleLoginInitialized = false;

                    try
                    {
                        FirebaseLoadDone();
                    }
                    catch {
                        Debug.LogError(string.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    }
                }
            });
#endif
        }

        private void OnFirebaseCheckAndAction(Action action)
        {
            if (action == null)
                return;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    action.Invoke();
                }
                else
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch {
                        Debug.LogError(string.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    }
                }
            });            
#endif
        }



        public void FirebaseLoadDone()
        {
            Firebase.DynamicLinks.DynamicLinks.DynamicLinkReceived += OnDynamicLink;

            //var components = new Firebase.DynamicLinks.DynamicLinkComponents(new System.Uri("https://dapps.meta-toy.world/saga"), "https://mtdz.page.link/")
            //{
            //    IOSParameters = new Firebase.DynamicLinks.IOSParameters("com.sandboxgame.mtdz"),
            //    AndroidParameters = new Firebase.DynamicLinks.AndroidParameters("com.sandboxgame.mtdz"),
            //};

            //var options = new Firebase.DynamicLinks.DynamicLinkOptions
            //{
            //    PathLength = Firebase.DynamicLinks.DynamicLinkPathLength.Unguessable
            //};

            //Firebase.DynamicLinks.DynamicLinks.GetShortLinkAsync(components, options).ContinueWith(task => {
            //    if (task.IsCanceled)
            //    {
            //        Debug.LogError("GetShortLinkAsync was canceled.");
            //        return;
            //    }
            //    if (task.IsFaulted)
            //    {
            //        Debug.LogError("GetShortLinkAsync encountered an error: " + task.Exception);
            //        return;
            //    }

            //    // Short Link has been created.
            //    Firebase.DynamicLinks.ShortDynamicLink link = task.Result;
            //    Debug.LogFormat("Generated short link {0}", link.Url);

            //    var warnings = new System.Collections.Generic.List<string>(link.Warnings);
            //    if (warnings.Count > 0)
            //    {
            //        // Debug logging for warnings generating the short link.
            //    }
            //});
        }

        void OnDynamicLink(object sender, System.EventArgs args)
        {
            var dynamicLinkEventArgs = args as Firebase.DynamicLinks.ReceivedDynamicLinkEventArgs;
            if (dynamicLinkEventArgs == null)
            {
                Debug.Log("Dynamic link cast failed.");
                return;
            }

            Debug.LogFormat("Received dynamic link {0}",
                            dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString);

            SetFirebaseEvent(dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString);
        }

        public void SetFirebaseEvent(string event_param)
        {
            OnFirebaseCheckAndAction(() => {
                Firebase.Analytics.FirebaseAnalytics.LogEvent(event_param);
            });
        }

        public void SetFirebaseEvent(string name, string parameterName, int parameterValue)
        {
            OnFirebaseCheckAndAction(() => {
                Firebase.Analytics.FirebaseAnalytics.LogEvent(name, parameterName, parameterValue);
            });
        }

        public void SetFirebaseEvent(string name, Firebase.Analytics.Parameter[] prms)
        {
            OnFirebaseCheckAndAction(() => {
                Firebase.Analytics.FirebaseAnalytics.LogEvent(name, prms);
            });
        }
        
        public void SetServerProperty(string serverName)
        {
            OnFirebaseCheckAndAction(() => {
                Firebase.Analytics.FirebaseAnalytics.SetUserProperty("ServerName", serverName);
            });
        }

        public void SetLanguageProperty()
        {
            OnFirebaseCheckAndAction(() => {
                Firebase.Analytics.FirebaseAnalytics.SetUserProperty("Language", Application.systemLanguage.ToString());
            });
        }
    }
}
