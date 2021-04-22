using FragmentServerWV.Entities;
using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Enumerations;
using FragmentServerWV.Services.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FragmentServerWV.Services
{
    public sealed class OpCodeProviderService : IOpCodeProviderService
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly List<Type> discoveredTypes;
        private readonly Dictionary<ushort, Type> opCodeProviders;
        private readonly Dictionary<(ushort, ushort), Type> opCodeDataProviders;

        public IReadOnlyCollection<Type> Handlers => discoveredTypes.AsReadOnly();

        public string ServiceName => "OpCode Provision Service";

        public ServiceStatusEnum ServiceStatus => ServiceStatusEnum.Active;



        public OpCodeProviderService(ILogger logger, IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.discoveredTypes = new List<Type>();
            this.opCodeProviders = new Dictionary<ushort, Type>();
            this.opCodeDataProviders = new Dictionary<(ushort, ushort), Type>();

            logger.Verbose($"Looking for implementations of {nameof(IOpCodeHandler)}");
            var handlerTypes = (from assemblies in AppDomain.CurrentDomain.GetAssemblies()
                                  let types = assemblies.GetTypes()
                                  let services = (from t in types
                                                  where typeof(IOpCodeHandler).IsAssignableFrom(t) &&
                                                  !t.IsAbstract && !t.IsInterface
                                                  select t)
                                  select services).SelectMany(c => c).ToList();
            var count = handlerTypes.Count;
            logger.Verbose("Done! Discovered {@count} instance(s)", count);

            foreach(var handler in handlerTypes)
            {
                var opCodeAttr = handler.GetCustomAttribute<OpCodeAttribute>();
                var dataOpCodeAttr = handler.GetCustomAttributes<OpCodeDataAttribute>();

                if (opCodeAttr is null)
                {
                    logger.Warning($"{{@handler}} was bypassed: No defined instance of {nameof(OpCodeAttribute)} available for query", handler);
                    continue;
                }

                switch(opCodeAttr.OpCode)
                {
                    case OpCodes.OPCODE_DATA:
                        if (((dataOpCodeAttr?.Count() ?? 0) == 0))
                        {
                            logger.Warning($"{{@handler}} was bypassed: No defined instance of {nameof(OpCodeDataAttribute)} available for query when necessary", handler);
                            continue;
                        }
                        foreach(var entry in dataOpCodeAttr)
                        {
                            if (entry.OpCode != OpCodes.OPCODE_DATA)
                            {
                                logger.Warning($"{{@handler}} was bypassed: The OPCODE used was NOT 0x30 (which is the DATA opcode) when 0x30 was expected", handler);
                            }
                            opCodeDataProviders.Add((OpCodes.OPCODE_DATA, entry.DataOpCode), handler);
                        }
                        break;
                    default:
                        opCodeProviders.Add(opCodeAttr.OpCode, handler);
                        break;
                }
                this.discoveredTypes.Add(handler);

            }
        }


        public async Task<ResponseContent> HandlePacketAsync(GameClientAsync gameClient, PacketAsync packet)
        {
            var requestContent = new RequestContent(gameClient, packet);
            IOpCodeHandler handler = null;
            switch(packet.Code)
            {
                case OpCodes.OPCODE_DATA:
                    var key = (OpCodes.OPCODE_DATA, requestContent.DataOpCode ?? throw new InvalidOperationException());
                    if (opCodeDataProviders.ContainsKey(key))
                    {
                        handler = serviceProvider.GetService(opCodeDataProviders[key]) as IOpCodeHandler;
                    }
                    break;
                default:
                    if (opCodeProviders.ContainsKey(packet.Code))
                    {
                        handler = serviceProvider.GetService(opCodeProviders[packet.Code]) as IOpCodeHandler;
                    }
                    break;
            }
            if (!(handler is null))
            {
                return await handler.HandleIncomingRequestAsync(requestContent);
            }
            return null;
        }

        public bool CanHandleRequest(PacketAsync packet)
        {
            var requestContent = new RequestContent(null, packet);
            switch (packet.Code)
            {
                case OpCodes.OPCODE_DATA:
                    var key = (OpCodes.OPCODE_DATA, requestContent.DataOpCode ?? throw new InvalidOperationException());
                    return opCodeDataProviders.ContainsKey(key);
                default:
                    return opCodeProviders.ContainsKey(packet.Code);
            }
        }

    }

}
