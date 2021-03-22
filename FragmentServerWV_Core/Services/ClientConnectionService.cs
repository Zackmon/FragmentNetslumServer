using FragmentServerWV.Services.Interfaces;
using Serilog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FragmentServerWV.Services
{
    public sealed class ClientConnectionService : IClientConnectionService
    {
        private readonly IClientProviderService clientProviderService;
        private readonly ILogger logger;
        private TcpListener listener;
        private CancellationTokenSource tokenSource;

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
        }

        public void EndListening()
        {
            logger.Information($"The {nameof(ClientConnectionService)} has had a shutdown requested");
            tokenSource.Cancel();
            logger.Information($"A cancellation request has been submitted");
        }



        private async Task InternalConnectionLoop(CancellationToken token)
        {
            listener.Start();
            uint clientIds = 1;
            try
            {

                using (token.Register(() => listener.Stop()))
                {
                    while (!token.IsCancellationRequested)
                    {
                        logger.Verbose("Invoking AcceptTcpClientAsync()");
                        var incomingConnection = await listener.AcceptTcpClientAsync();
                        logger.Verbose($"AcceptTcpClientAsync() has returned with a client, migrating to {nameof(IClientProviderService)}");
                        clientProviderService.AddClient(incomingConnection, clientIds++);
                        logger.Verbose("Performing Cancellation Token check...");
                        token.ThrowIfCancellationRequested();
                        logger.Verbose("Performing Cancellation Token check...passed");
                    }
                }

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
                token.ThrowIfCancellationRequested();
                logger.Error(ioe, $"The {nameof(ClientConnectionService)} was not told to shutdown. Please present this log to someone to investigate what went wrong while executing the code");
            }
            catch (OperationCanceledException oce)
            {
                logger.Error(oce, $"The {nameof(ClientConnectionService)} was told to explicitly shutdown and no further action is necessary");
            }
            finally
            {
                // At the end of it all, we need to remove the reference
                // on the listener variable and allow GC to reclaim it
                listener = null;
            }
        }

    }
}
