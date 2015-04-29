﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using TomP2P.Core.Connection;
using TomP2P.Core.Peers;
using TomP2P.Core.Rpc;

namespace TomP2P.Dht
{
    public class StorageRpc : DispatchHandler
    {
        public static readonly SimpleBloomFilter<Number160> EmptyFilter = new SimpleBloomFilter<Number160>(0, 0);
        public static readonly SimpleBloomFilter<Number160> FullFilter = new SimpleBloomFilter<Number160>(8, 1);
        
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Random Rnd = new Random(); // TODO InteropRandom required?

        private readonly IBloomfilterFactory _bloomfilterFactory;
        private readonly StorageLayer _storageLayer;
        private readonly IReplicationListener _replicationListener;

        // static constructor
        static StorageRpc()
        {
            FullFilter.SetAll();
        }

        public StorageRpc(PeerBean peerBean, ConnectionBean connectionBean, StorageLayer storageLayer)
            : base(peerBean, connectionBean)
        {
            Register(
                Rpc.Commands.Put.GetNr(),
                Rpc.Commands.Get.GetNr(),
                Rpc.Commands.Add.GetNr(),
                Rpc.Commands.Remove.GetNr(),
                Rpc.Commands.Digest.GetNr(),
                Rpc.Commands.DigestBloomfilter.GetNr(),
                Rpc.Commands.DigestAllBloomfilter.GetNr(),
                Rpc.Commands.Put.GetNr(),
                Rpc.Commands.Put.GetNr(),
                Rpc.Commands.Put.GetNr());
        }

        public override void HandleResponse(Core.Message.Message requestMessage, Core.Connection.PeerConnection peerConnection, bool sign, Core.Connection.IResponder responder)
        {
            throw new NotImplementedException();
        }
    }
}
