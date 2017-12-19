using UnityEngine;

namespace XLuaExtension
{
    [DisallowMultipleComponent]
    public class CollisionMessage : _Message<CollisionMessage>
    {
        public class CollisionEvent : OnMessageEvent<Collision> { }

        public CollisionEvent onCollisionEnter = new CollisionEvent();
        public CollisionEvent onCollisionStay = new CollisionEvent();
        public CollisionEvent onCollisionExit = new CollisionEvent();

        void OnCollisionEnter(Collision collision)
        {
            onCollisionEnter.Invoke(collision);
        }
        void OnCollisionStay(Collision collision)
        {
            onCollisionStay.Invoke(collision);
        }
        void OnCollisionExit(Collision collision)
        {
            onCollisionExit.Invoke(collision);
        }
    }
}