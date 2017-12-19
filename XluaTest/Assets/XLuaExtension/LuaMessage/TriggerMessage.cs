using UnityEngine;

namespace XLuaExtension
{
    [DisallowMultipleComponent]
    public class TriggerMessage : _Message<TriggerMessage>
    {
        public class TriggerEvent : OnMessageEvent<Collider> { }

        public TriggerEvent onTriggerEnter = new TriggerEvent();
        public TriggerEvent onTriggerStay = new TriggerEvent();
        public TriggerEvent onTriggerExit = new TriggerEvent();

        void OnTriggerEnter(Collider collider)
        {
            onTriggerEnter.Invoke(collider);
        }
        void OnTriggerStay(Collider collider)
        {
            onTriggerStay.Invoke(collider);
        }
        void OnTriggerExit(Collider collider)
        {
            onTriggerExit.Invoke(collider);
        }
    }
}