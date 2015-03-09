using System;

namespace FoxytNetworking.Event
{
    public abstract class BaseEventArgs : EventArgs
    {
        public Session.Session Session { get; protected set; }

        public BaseEventArgs(Session.Session session)
        {
            this.Session = session;
        }

        public T GetTypedSession<T>()
            where T : Session.Session
        {
            return this.Session as T;
        }
    }
}
