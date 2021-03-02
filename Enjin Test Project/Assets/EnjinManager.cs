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

        User _developementUser = null;
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

                _developementUser = Enjin.GetUser(_enjinUIManager.UserId);

                if (_developementUser == null) return;

                _enjinUIManager.DisableUserLoginUI();
                Debug.Log($"[Logined User ID] {_developementUser.id}");
                Debug.Log($"[Logined User name] {_developementUser.name}");

                _accessToken = Enjin.AccessToken;

                if (_accessToken == null) return;

                _enjinUIManager.AccessToken = _accessToken;
            });
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
                    _enjinUIManager.DisableAppLoginUI();
                    _enjinUIManager.EnableUserLoginUI();
                    yield break;
                }

                tick++;
                yield return waitASecond;
            }

            Debug.Log("App auth Faild");
            _isConnecting = false;

            yield return null;
        }
    }
}

