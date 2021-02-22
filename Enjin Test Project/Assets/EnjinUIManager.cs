using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class EnjinUIManager : MonoBehaviour
{
    [SerializeField] Button _loginPlatformButton;
    [SerializeField] PlatformSelector _platformSelector;
    [SerializeField] InputField _appIdInputField;
    [SerializeField] InputField _appSecretInputField;

    public void RegisterAppLoginEvent(UnityAction action)
    {
        _loginPlatformButton.onClick.AddListener(action);
    }

    public int GetAppId()
    {
        return System.Convert.ToInt32(_appIdInputField.text);
    }

    public string GetAppSecret()
    {
        return _appSecretInputField.text;
    }

    public string GetPlatformURL()
    {
        return _platformSelector.GetPlatformURL();
    }

    public void DisableAppLoginUI()
    {
        _loginPlatformButton.gameObject.SetActive(false);
        _appIdInputField.gameObject.SetActive(false);
        _appSecretInputField.gameObject.SetActive(false);
    }
}
