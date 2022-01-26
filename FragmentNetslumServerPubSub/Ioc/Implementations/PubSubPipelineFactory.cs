using FragmentNetslumServerPubSub.Core;
using FragmentNetslumServerPubSub.Ioc.Interfaces;

namespace FragmentNetslumServerPubSub.Ioc.Implementations
{
    public class PubSubPipelineFactory : IPubSubPipelineFactory
    {
        private readonly Hub hub;

        public PubSubPipelineFactory()
        {
            hub = new Hub();
        }

        public IPublisher GetPublisher() => new Publisher( hub );

        public ISubscriber GetSubscriber() => new Subscriber( hub );
    }
}