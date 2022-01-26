using FragmentNetslumServerPubSub.Core;
using FragmentNetslumServerPubSub.Ioc.Interfaces;

namespace FragmentNetslumServerPubSub.Ioc.Implementations
{
    
    public class Publisher : IPublisher
    {
        private readonly Hub hub;

        public Publisher( Hub hub )
        {
            this.hub = hub;
        }

        public void Publish<T>(T data) => hub.Publish(data);
    }

}