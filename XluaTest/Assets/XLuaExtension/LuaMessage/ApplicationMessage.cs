using UnityEngine;

namespace XLuaExtension
{
    [DisallowMultipleComponent]
    public class ApplicationMessage : _Message<ApplicationMessage>
    {
        public class ApplicationEvent : OnMessageEvent<bool> { }

        public ApplicationEvent onApplicationFocus = new ApplicationEvent();
        public ApplicationEvent onApplicationPause = new ApplicationEvent();

        void OnApplicationFocus(bool focusStatus)
        {
            onApplicationFocus.Invoke(focusStatus);
        }
        void OnApplicationPause(bool pauseStatus)
        {
            onApplicationPause.Invoke(pauseStatus);
        }
    }
}