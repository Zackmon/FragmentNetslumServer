using FragmentNetslumServer.Enumerations;

namespace FragmentNetslumServer.Services.Interfaces
{

    /// <summary>
    /// Defines basic information that all services should be able to provide
    /// </summary>
    public interface IBaseService
    {

        /// <summary>
        /// Gets a friendly display name of the service
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Gets the current status of the service
        /// </summary>
        ServiceStatusEnum ServiceStatus { get; }

    }

}