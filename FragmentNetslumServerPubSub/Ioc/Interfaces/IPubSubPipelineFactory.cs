namespace FragmentNetslumServerPubSub.Ioc.Interfaces
{

    public interface IPubSubPipelineFactory
    {
        IPublisher GetPublisher();
        ISubscriber GetSubscriber();
    }

}