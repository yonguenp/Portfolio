package com.sandbox.gs.samanda;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import com.google.android.gms.auth.api.signin.GoogleSignIn;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.auth.api.signin.GoogleSignInClient;
import com.google.android.gms.auth.api.signin.GoogleSignInOptions;
import com.google.android.gms.common.api.ApiException;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;
import com.google.android.gms.tasks.Task;
import com.google.gson.Gson;
import com.unity3d.player.UnityPlayer;

import java.util.HashMap;
import java.util.Map;

public class GoogleSignInHandlerActivity extends AppCompatActivity {
    private static final String TAG = "oAuth";
    private static final int RC_SIGN_IN = 9001;
    private static String url = "622508035915-kja1fh96a2em8s5q1c7psf2nrqk6is2u.apps.googleusercontent.com";
    private GoogleSignInClient mGoogleSignInClient;

    public static void onReqOAuth(final Activity current) {
        Log.d(TAG, "onReqOAuth");
        Log.d(TAG, "try Signout first");
        doSignOut(current, true);
    }

    public static void setURL(final String target_url)
    {
        url = target_url;
    }

    public static void onReqSignOut(final Activity current) {
        doSignOut(current, false);
    }

    public static void doSignIn(final Activity current) {
        Log.d(TAG, "doSignIn called");
        Intent intent = new Intent(current, GoogleSignInHandlerActivity.class);
        intent.addFlags (Intent.FLAG_ACTIVITY_NO_ANIMATION);
        current.startActivity(intent);
    }

    private static void doSignOut(final Activity current, final boolean signInAgain) {
        getGoogleClient(current).signOut()
                .addOnSuccessListener(current, new OnSuccessListener<Void>() {
                    @Override
                    public void onSuccess(Void aVoid) {
                        Log.d(TAG, "Sign out success");
                    }
                })
                .addOnFailureListener(current, new OnFailureListener() {
                    @Override
                    public void onFailure(@NonNull Exception e) {
                        Log.e(TAG, "Sign out failed", e);
                    }
                })
                .addOnCompleteListener(current, new OnCompleteListener<Void>() {
                    @Override
                    public void onComplete(@NonNull Task<Void> task) {
                        Log.d(TAG, "signout onComplete");
                        if (signInAgain) {
                            Log.d(TAG, "sign in again");
                            GoogleSignInHandlerActivity.doSignIn(current);
                        }
                    }
                });
    }

    private static GoogleSignInClient getGoogleClient(final Activity current) {
        GoogleSignInOptions gso = new GoogleSignInOptions
                .Builder(GoogleSignInOptions.DEFAULT_SIGN_IN)
                .requestIdToken(url)
                .build();
        return GoogleSignIn.getClient(current, gso);
    }

    @Override
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);

//        GoogleSignInOptions gso = new GoogleSignInOptions
//                .Builder(GoogleSignInOptions.DEFAULT_SIGN_IN)
//                .requestIdToken("826055537016-5runs8gjsr2euabgc9c0405081h2apvq.apps.googleusercontent.com")
//                .build();
        mGoogleSignInClient = getGoogleClient(this);

        trySignInOnCreate();
    }

    private void trySignInOnCreate() {
        Intent intent = mGoogleSignInClient.getSignInIntent();
        startActivityForResult(intent, RC_SIGN_IN);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, @Nullable Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (RC_SIGN_IN == requestCode) {
            Task<GoogleSignInAccount> task = GoogleSignIn.getSignedInAccountFromIntent(data);
            onSignInResult(task);
        } else {
            callSignInCallback(1, "");
        }

        finish();
    }

    private void onSignInResult(Task<GoogleSignInAccount> task) {
        try {
            GoogleSignInAccount account = task.getResult(ApiException.class);

            if (null != account) {
                callSignInCallback(0, account.getIdToken());
            } else {
                callSignInCallback(1, "");
            }
        } catch (ApiException e) {
            callSignInCallback(e.getStatusCode(), "");
        }
    }

    private void callSignInCallback(int code, String token) {
        Map<String, String> data = new HashMap<String, String>();
        data.put("rs", Integer.toString(code));
        data.put("type", "GG");
        data.put("token", token);

        Gson gson = new Gson();
        String param = "window.Samanda.onOAuthJavaResponse('"
                + gson.toJson(data)
                + "')";

        // if unity
        UnityPlayer.UnitySendMessage("WebViewObject", "EvaluateJS", param);
    }

    @Override
    protected void onPause() {
        super.onPause();
        overridePendingTransition(0,0);
        //액티비티 애니메이션 x
    }
}
