namespace FragmentServerWV_PubSub.Ioc.Interfaces
{

    public interface IPubSubPipelineFactory
    {
        IPublisher GetPublisher();
        ISubscriber GetSubscriber();
    }

}