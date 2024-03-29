﻿using FragmentNetslumServer.Entities;
using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Enumerations;
using FragmentNetslumServer.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FragmentNetslumServer.Services
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
                var opCodeAttrs = handler.GetCustomAttributes<OpCodeAttribute>();
                var dataOpCodeAttr = handler.GetCustomAttributes<OpCodeDataAttribute>();

                if (opCodeAttrs is null || !opCodeAttrs.Any())
                {
                    logger.Warning($"{{@handler}} was bypassed: No defined instance of {nameof(OpCodeAttribute)} available for query", handler);
                    continue;
                }


                switch(opCodeAttrs.First().OpCode)
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
                        opCodeProviders.Add(opCodeAttrs.First().OpCode, handler);
                        break;
                }
                this.discoveredTypes.Add(handler);

            }
        }



        public async Task<IEnumerable<ResponseContent>> HandlePacketAsync(GameClientAsync gameClient, PacketAsync packet)
        {
            var requestContent = new RequestContent(gameClient, packet);
            IOpCodeHandler handler = null;
            switch(packet.Code)
            {
                case OpCodes.OPCODE_DATA:
                    var key = (OpCodes.OPCODE_DATA, requestContent.DataOpCode ?? throw new InvalidOperationException());
                    if (opCodeDataProviders.ContainsKey(key))
                    {
                        logger.LogData(packet.Data, (ushort)requestContent.DataOpCode, (int)gameClient.ClientIndex, nameof(HandlePacketAsync), packet.ChecksumInPacket, packet.ChecksumOfPacket);
                        handler = ActivatorUtilities.CreateInstance(serviceProvider, opCodeDataProviders[key]) as IOpCodeHandler;
                    }
                    break;
                default:
                    if (opCodeProviders.ContainsKey(packet.Code))
                    {
                        handler = ActivatorUtilities.CreateInstance(serviceProvider, opCodeProviders[packet.Code]) as IOpCodeHandler;
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
