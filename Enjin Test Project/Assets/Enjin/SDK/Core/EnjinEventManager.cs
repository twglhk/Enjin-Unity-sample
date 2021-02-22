using System;
using System.Collections.Generic;

namespace Enjin.SDK.Core
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    //public class EnjinAction<T>
    //{
    //    public Action<T> Action { get; set; }

    //    public static Action<T> operator + (EnjinAction<T> a1, EnjinAction<T> a2)
    //    {
    //        return a1.Action = <T> => a2.Action;
    //    }
    //}

    /// <summary>
    /// Event Manager replacement model using C# Action (Delegate) model versus UnityEvent model.
    /// Abstract Action is used in the dictionary to provide support for elements beyond the SDK
    /// allowing this Event Manager to be used by clients in their applications for events outside
    /// of the Enjin SDK scope.
    /// 
    /// Similiarly, the Event Manager is provided with MonoBehavior support allowing it to be used
    /// with game objects.
    /// </summary>
    public class EnjinEventManager //: MonoBehaviour
    {
        #region Private Vars

        private static EnjinEventManager _enjinEventManager;
        private Dictionary<string, Action<RequestEvent>> _requestEventDictionary;

        #endregion

        #region Manager and Init Methods

        /// <summary>
        /// Create a new unique manager instance if one does not already exist
        /// </summary>
        public static EnjinEventManager instance
        {
            get
            {
                if (_enjinEventManager == null)
                {
                    // To use this event manager as an object attached script, add : MonoBehaviour to the class 
                    // declaration.
#if UNITY_EDITOR
                    //if (Application.isPlaying)
                    //{
                    //    _enjinEventManager = FindObjectOfType(typeof(EnjinEventManager)) as EnjinEventManager;
                    //}
                    // and comment out the following.
#endif
                    if (_enjinEventManager == null)
                    {
                        _enjinEventManager = new EnjinEventManager();
                        _enjinEventManager.Init();
                    }
                }

                return _enjinEventManager;
            }
        }

        /// <summary>
        /// Initialize the event and action dictionaries
        /// </summary>
        private void Init()
        {
            if (_requestEventDictionary == null)
            {
                _requestEventDictionary = new Dictionary<string, Action<RequestEvent>>();
            }
        }

        #endregion

        #region General Methods

        public void StartListening(string eventName, Action<RequestEvent> listener)
        {
            Action<RequestEvent> thisEvent;
            if (instance._requestEventDictionary.TryGetValue(eventName, out thisEvent))
            {
                // add to the current event.
                thisEvent += listener;

                // update the dictionary now.
                instance._requestEventDictionary[eventName] = thisEvent;
            }
            else
            {
                // add the event to the dictionary (first pass)
                thisEvent += listener;
                instance._requestEventDictionary.Add(eventName, thisEvent);
            }
        }

        public void StopListening(string eventName, Action<RequestEvent> listener)
        {
            if (_enjinEventManager == null) return;
            Action<RequestEvent> thisEvent;
            if (instance._requestEventDictionary.TryGetValue(eventName, out thisEvent))
            {
                // remove event
                thisEvent -= listener;

                // update the dictionary
                instance._requestEventDictionary[eventName] = thisEvent;
            }
        }

        public Action<RequestEvent> FetchAction(string eventName)
        {
            Action<RequestEvent> thisAction = null;
            if (instance._requestEventDictionary.TryGetValue(eventName, out thisAction))
            {
                return thisAction;
            }

            return null;
        }

        public void TriggerEvent(string eventName, RequestEvent request)
        {
            Action<RequestEvent> thisAction = null;
            if (instance._requestEventDictionary.TryGetValue(eventName, out thisAction))
            {
                thisAction.Invoke(request);
            }
        }

        #endregion
    }
}