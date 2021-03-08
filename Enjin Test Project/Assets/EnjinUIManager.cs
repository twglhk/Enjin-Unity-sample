using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Enjin.SDK.Core;

public class EnjinUIManager : MonoBehaviour
{
    [SerializeField] PlatformSelector _platformSelector;

    [SerializeField] Canvas _loginAppCanvas;
    [SerializeField] Button _loginAppButton;
    [SerializeField] InputField _appIdInputField;
    [SerializeField] InputField _appSecretInputField;
    
    [SerializeField] Canvas _loginUserCanvas;
    [SerializeField] Button _loginUserButton;
    [SerializeField] InputField _userName;

    [SerializeField] Canvas _identityCanvas;
    [SerializeField] Button _queryIdentityButton;
    [SerializeField] Text _identityText;

    [SerializeField] Text _loggedInAppId;
    [SerializeField] Text _loggedInUserName;
    [SerializeField] Text _accessToken;

    public int EnjinAppId
    {
        get { return System.Convert.ToInt32(_appIdInputField.text); }
        set { _loggedInAppId.text = value.ToString(); }
    }

    public string EnjinAppSecret
    {
        get { return _appSecretInputField.text; }
    }

    public string EnjinPlatformURL
    {
        get { return _platformSelector.GetPlatformURL(); }
    }

    public string UserName
    {
        get { return _userName.text; }
        set { _loggedInUserName.text = value; }
    }

    public string AccessToken
    {
        get { return _accessToken.text; }
        set { _accessToken.text = value; }
    }

    public void RegisterAppLoginEvent(UnityAction action)
    {
        _loginAppButton.onClick.AddListener(action);
    }

    public void RegisterUserLoginEvent(UnityAction action)
    {
        _loginUserButton.onClick.AddListener(action);
    }

    public void RegisterGetIdentityEvent(UnityAction action)
    {
        _queryIdentityButton.onClick.AddListener(action);
    }

    public void EnableUserLoginUI()
    {
        _loginUserCanvas.enabled = true;
    }

    public void DisableAppLoginUI()
    {
        _loginAppCanvas.enabled = false;
        _platformSelector.DisableInteractable();
    }

    public void DisableUserLoginUI()
    {
        _loginUserCanvas.enabled = false;
        _identityCanvas.enabled = true;
    }
}
