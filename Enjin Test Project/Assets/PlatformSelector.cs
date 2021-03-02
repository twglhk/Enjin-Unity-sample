using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlatformSelector : MonoBehaviour
{
    private static readonly string _kovanURL = "https://kovan.cloud.enjin.io/";
    private static readonly string _mainnetURL = "https://cloud.enjin.io/";
    [SerializeField] private Toggle _kovanToggle;
    [SerializeField] private Toggle _mainnetToggle;

    public string GetPlatformURL()
    {
        if (_kovanToggle.isOn)
            return _kovanURL;
        else if (_mainnetToggle.isOn)
            return _mainnetURL;
        else
        {
            Debug.LogError("Platform selection error");
            return null;
        }
    }

    public void DisableInteractable()
    {
        _kovanToggle.interactable = false;
        _mainnetToggle.interactable = false;
    }
}
