using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Enjin.SDK.Core
{
    public class EnjinManager : MonoBehaviour
    {
        [SerializeField] private EnjinUIManager _enjinUIManager;

        private void Start()
        {
            Enjin.IsDebugLogActive = true;
            _enjinUIManager.RegisterAppLoginEvent(() => 
            {
                if (Enjin.LoginState == LoginState.VALID)
                    return;

                StartCoroutine(AppLoginRoutine());
            });
        }

        private IEnumerator AppLoginRoutine()
        {
            Enjin.StartPlatform(_enjinUIManager.GetPlatformURL(),
                _enjinUIManager.GetAppId(), _enjinUIManager.GetAppSecret());

            int tick = 0;
            YieldInstruction waitASecond = new WaitForSeconds(1f);
            while (tick < 10)
            {
                if (Enjin.LoginState == LoginState.VALID)
                {
                    Debug.Log("App auth success");
                    _enjinUIManager.DisableAppLoginUI();
                    yield break;
                }

                tick++;
                yield return waitASecond;
            }

            Debug.Log("App auth Faild");

            yield return null;
        }
    }
}

