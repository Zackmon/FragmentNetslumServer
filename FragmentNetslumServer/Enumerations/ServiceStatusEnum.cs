namespace FragmentNetslumServer.Enumerations
{

    /// <summary>
    /// Defines the various states of execution that a service can be in
    /// </summary>
    public enum ServiceStatusEnum
    {
        /// <summary>
        /// The service is currently inactive awaiting activation
        /// </summary>
        Inactive,

        /// <summary>
        /// The service is currently active and running properly
        /// </summary>
        Active,

        /// <summary>
        /// The service has experienced an error of some sort and needs to be addressed
        /// </summary>
        Faulted
    }

}
