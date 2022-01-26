using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FragmentNetslumServer.Enumerations;
using FragmentNetslumServer.Services.Interfaces;
using Serilog;

namespace FragmentNetslumServer.Services
{
    public sealed class ClientConnectionService : IClientConnectionService
    {
        private readonly IClientProviderService clientProviderService;
        private readonly ILogger logger;
        private TcpListener listener;
        private CancellationTokenSource tokenSource;

        private IPAddress lastIpAddress;
        private ushort lastPort;

        public string ServiceName => "Client Connection Service";

        public ServiceStatusEnum ServiceStatus { get; private set; }


        public ClientConnectionService(
            IClientProviderService clientProviderService,
            ILogger logger)
        {
            this.clientProviderService = clientProviderService;
            this.logger = logger;
        }

        public void BeginListening(string ipAddress, ushort port)
        {
            if (!IPAddress.TryParse(ipAddress, out var ip)) throw new ArgumentException(nameof(ipAddress));
            this.BeginListening(ip, port);
        }

        public void BeginListening(IPAddress ipAddress, ushort port)
        {
            if (listener != null)
            {
                throw new NotSupportedException($"Multiple calls to {nameof(BeginListening)} is not supported");
            }
            logger.Information("Opening TCP Listener on {@ipAddress}:{@port}", ipAddress.ToString(), port);
            tokenSource = new CancellationTokenSource();
            listener = new TcpListener(ipAddress, port);
            Task.Run(async () => await InternalConnectionLoop(tokenSource.Token));
            this.lastIpAddress = ipAddress;
            this.lastPort = port;
        }

        public void EndListening()
        {
            logger.Information($"The {nameof(ClientConnectionService)} has had a shutdown requested");
            tokenSource.Cancel();
            logger.Information($"A cancellation request has been submitted");
        }

        public void RestartService()
        {
            // Don't restart if we're already running
            // or if the last known IP address is null
            if (this.ServiceStatus == ServiceStatusEnum.Active)
            {
                throw new NotSupportedException("As ironic as it seems, you cannot currently restart the service while it is running");
            }
            if (this.lastIpAddress is null)
            {
                throw new InvalidOperationException("The service has not been run before and has no last known IP address to re-bind to");
            }
            this.logger.Information($"The {nameof(ClientConnectionService)} is being restarted...");
            this.BeginListening(lastIpAddress, lastPort);
            this.logger.Information($"The {nameof(ClientConnectionService)} has been restarted");
        }

        private async Task InternalConnectionLoop(CancellationToken token)
        {
            listener.Start();
            uint clientIds = 1;
            using (token.Register(() =>
            {
                this.ServiceStatus = ServiceStatusEnum.Inactive;
                listener.Stop();
                listener = null;
            }))
            {
                this.ServiceStatus = ServiceStatusEnum.Active;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        logger.Verbose("Invoking AcceptTcpClientAsync()");
                        var incomingConnection = await listener.AcceptTcpClientAsync();
                        logger.Verbose($"AcceptTcpClientAsync() has returned with a client, migrating to {nameof(IClientProviderService)}");
                        clientProviderService.AddClient(incomingConnection, clientIds++);
                        logger.Verbose("Client Provider Service now has the client");
                    }
                    catch (ObjectDisposedException ode)
                    {
                        logger.Error(ode, "The service was told to shutdown, or errored, after an incoming connection attempt was made. It is probably safe to ignore this Error as the listener is already shutting down");
                    }
                    catch (InvalidOperationException ioe)
                    {
                        // Either tcpListener.Start wasn't called (a bug!)
                        // or the CancellationToken was cancelled before
                        // we started accepting (giving an InvalidOperationException),
                        // or the CancellationToken was cancelled after
                        // we started accepting (giving an ObjectDisposedException).
                        //
                        // In the latter two cases we should surface the cancellation
                        // exception, or otherwise rethrow the original exception.
                        logger.Error(ioe, $"The {nameof(ClientConnectionService)} was told to shutdown, or errored, before an incoming connection attempt was made. More context is necessary to see if this Error can be safely ignored");
                        if (token.IsCancellationRequested)
                        {
                            logger.Error(ioe, $"The {nameof(ClientConnectionService)} was told to shutdown.");
                        }
                        else
                        {
                            logger.Error(ioe, $"The {nameof(ClientConnectionService)} was not told to shutdown. Please present this log to someone to investigate what went wrong while executing the code");
                        }
                    }
                    catch (OperationCanceledException oce)
                    {
                        logger.Error(oce, $"The {nameof(ClientConnectionService)} was told to explicitly shutdown and no further action is necessary");
                    }
                }
                this.ServiceStatus = ServiceStatusEnum.Inactive;
            }
        }
    }
}
