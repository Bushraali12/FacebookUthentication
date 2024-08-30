using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Facebook.Unity;
using System;

public class FacebookLogin : MonoBehaviour
{
    public TextMeshProUGUI FB_userName;
    public RawImage rawImg;

    #region Initialize

    private void Awake()
    {
        if (!FB.IsInitialized)
        {
            StartCoroutine(WaitForFacebookInit());
        }
        else
        {
            FB.ActivateApp();
            SetInit();
        }
    }

    IEnumerator WaitForFacebookInit()
    {
        FB.Init(OnFacebookInitialized, OnHideUnity);

        while (!FB.IsInitialized)
        {
            yield return null;
        }
        
        FB.ActivateApp();
        SetInit();
    }

    void OnFacebookInitialized()
    {
        if (FB.IsInitialized)
        {
            Debug.Log("Facebook SDK initialized successfully.");
            FB.ActivateApp();
        }
        else
        {
            Debug.LogError("Failed to initialize the Facebook SDK.");
        }
    }

    void SetInit()
    {
        if (FB.IsLoggedIn)
        {
            Debug.Log("Facebook is logged in!");
            Debug.Log("Client token: " + FB.ClientToken);
            Debug.Log("User ID: " + AccessToken.CurrentAccessToken.UserId);
            Debug.Log("Token string: " + AccessToken.CurrentAccessToken.TokenString);
        }
        else
        {
            Debug.Log("Facebook is not logged in!");
        }
        DealWithFbMenus(FB.IsLoggedIn);
    }

    void OnHideUnity(bool isGameShown)
    {
        Time.timeScale = isGameShown ? 1 : 0;
    }

    void DealWithFbMenus(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            FB.API("/me?fields=first_name", HttpMethod.GET, DisplayUsername);
            FB.API("/me/picture?type=square&height=128&width=128", HttpMethod.GET, DisplayProfilePic);
        }
        else
        {
            Debug.Log("Not logged in");
        }
    }

    void DisplayUsername(IResult result)
    {
        if (result == null || !string.IsNullOrEmpty(result.Error))
        {
            Debug.LogError(result?.Error ?? "Error retrieving username");
            return;
        }

        if (result.ResultDictionary.TryGetValue("first_name", out var firstName))
        {
            string name = firstName.ToString();
            FB_userName.text = name;
            Debug.Log("Username: " + name);
        }
    }

    void DisplayProfilePic(IGraphResult result)
    {
        if (result == null || !string.IsNullOrEmpty(result.Error))
        {
            Debug.LogError(result?.Error ?? "Error retrieving profile picture");
            return;
        }

        if (result.Texture != null)
        {
            rawImg.texture = result.Texture;
            Debug.Log("Profile picture updated");
        }
    }

    #endregion

    // Login
    public void Facebook_LogIn()
    {
        List<string> permissions = new List<string> { "public_profile" };
        FB.LogInWithReadPermissions(permissions, AuthCallBack);
    }

    void AuthCallBack(IResult result)
    {
        if (FB.IsLoggedIn)
        {
            SetInit();
            var aToken = AccessToken.CurrentAccessToken;

            Debug.Log("User ID: " + aToken.UserId);
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log("Permission: " + perm);
            }
        }
        else
        {
            Debug.LogError("Failed to log in");
        }
    }

    // Logout
    public void Facebook_LogOut()
    {
        StartCoroutine(LogOut());
    }

    IEnumerator LogOut()
    {
        FB.LogOut();
        while (FB.IsLoggedIn)
        {
            Debug.Log("Logging out...");
            yield return null;
        }
        Debug.Log("Logout successful");

        if (FB_userName != null) FB_userName.text = "";
        if (rawImg != null) rawImg.texture = null;
    }

    #region Other

    public void FacebookSharefeed()
    {
        string url = "https://developers.facebook.com/docs/unity/reference/current/FB.ShareLink";
        FB.ShareLink(
            new Uri(url),
            "Checkout COCO 3D channel",
            "I just watched " + "22" + " times of this channel",
            null,
            ShareCallback);
    }

    private static void ShareCallback(IShareResult result)
    {
        Debug.Log("ShareCallback");
        SpentCoins(2, "sharelink");
        if (result.Error != null)
        {
            Debug.LogError(result.Error);
            return;
        }
        Debug.Log(result.RawResult);
    }

    public static void SpentCoins(int coins, string item)
    {
        var param = new Dictionary<string, object>
        {
            [AppEventParameterName.ContentID] = item
        };
        FB.LogAppEvent(AppEventName.SpentCredits, (float)coins, param);
    }

    #endregion
}
