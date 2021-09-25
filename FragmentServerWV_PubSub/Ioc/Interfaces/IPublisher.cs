namespace FragmentServerWV_PubSub.Ioc.Interfaces
{

    /// <summary>
    /// One part of the pub-sub model. Provides a meaningful definition for a publisher to notify
    /// subscribers that an event or action has occurred that they should be interested in
    /// </summary>
    public interface IPublisher
    {

        /// <summary>
        /// Publishes an object instance to all listeners.
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <remarks>
        /// If <typeparamref name="T"/> is a reference type, ALL subscribers (listeners) will receive the SAME instance
        /// </remarks>
        void Publish<T>(T data);

    }
    
}