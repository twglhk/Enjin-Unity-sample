using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enjin.SDK.DataTypes;

namespace Enjin.SDK.Core
{
    public class EnjinManager : MonoBehaviour
    {
        [SerializeField] private EnjinUIManager _enjinUIManager;

        User _currentEnjinUser = null;
        bool _isConnecting = false;
        string _accessToken = null;

        private void Awake()
        {
            Enjin.IsDebugLogActive = true;
        }

        private void Start()
        {
            _enjinUIManager.RegisterAppLoginEvent(() => 
            {
                if (Enjin.LoginState == LoginState.VALID)
                    return;

                if (_isConnecting)
                    return;

                _isConnecting = true;
                StartCoroutine(AppLoginRoutine());
            });

            _enjinUIManager.RegisterUserLoginEvent(() =>
            {
                if (Enjin.LoginState != LoginState.VALID)
                    return;

                _currentEnjinUser = Enjin.GetUser(_enjinUIManager.UserName);
                _accessToken = Enjin.AccessToken;

                Debug.Log($"[Logined User ID] {_currentEnjinUser.id}");
                Debug.Log($"[Logined User name] {_currentEnjinUser.name}");

                _enjinUIManager.UserName = _currentEnjinUser.name;
                _enjinUIManager.AccessToken = _accessToken;
                _enjinUIManager.DisableUserLoginUI();
            });

            _enjinUIManager.RegisterGetIdentityEvent(() =>
            {
                if (Enjin.LoginState != LoginState.VALID)
                    return;

                for (int i = 0; i < _currentEnjinUser.identities.Length; ++i)
                {
                    Debug.Log($"[{i} Identity ID] {_currentEnjinUser.identities[i].id}");
                    Debug.Log($"[{i} Identity linking Code] {_currentEnjinUser.identities[i].linkingCode}");
                    Debug.Log($"[{i} Identity Wallet :: Eth Address] {_currentEnjinUser.identities[i].wallet.ethAddress}");
                }
            });

            // TO DO : BindEvent from pusher
        }

        private IEnumerator AppLoginRoutine()
        {
            Enjin.StartPlatform(_enjinUIManager.EnjinPlatformURL,
                _enjinUIManager.EnjinAppId, _enjinUIManager.EnjinAppSecret);

            int tick = 0;
            YieldInstruction waitASecond = new WaitForSeconds(1f);
            while (tick < 10)
            {
                if (Enjin.LoginState == LoginState.VALID)
                {
                    Debug.Log("App auth success");
                    _enjinUIManager.EnjinAppId = Enjin.AppID;
                    _enjinUIManager.DisableAppLoginUI();
                    _enjinUIManager.EnableUserLoginUI();
                    yield break;
                }

                tick++;
                yield return waitASecond;
            }

            Debug.Log("App auth Failed");
            _isConnecting = false;

            yield return null;
        }
    }
}

