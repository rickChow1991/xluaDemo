using UnityEngine;

namespace XLuaExtension
{
    [DisallowMultipleComponent]
    public class UpdateMessage : _Message<UpdateMessage>
    {
        public class UpdateEvent : OnMessageEvent { }

        public UpdateEvent update = new UpdateEvent();
        public UpdateEvent fixedUpdate = new UpdateEvent();
        public UpdateEvent lateUpdate = new UpdateEvent();

        void Update()
        {
            update.Invoke();
        }
        void FixedUpdate()
        {
            fixedUpdate.Invoke();
        }
        void LateUpdate()
        {
            lateUpdate.Invoke();
        }
    }
}