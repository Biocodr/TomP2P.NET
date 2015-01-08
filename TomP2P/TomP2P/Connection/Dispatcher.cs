﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using NLog;
using TomP2P.Connection.Windows;
using TomP2P.Peers;
using TomP2P.Rpc;

namespace TomP2P.Connection
{
    /// <summary>
    /// Used to deliver incoming REQUEST messages to their specific handlers.
    /// Handlers can be registered using the RegisterIoHandler function.
    /// <para>
    /// (You probably want to add an instance of this class to the end of a pipeline to be able to receive messages.
    /// This class is able to cover several channels but only one P2P network!)
    /// </para>
    /// </summary>
    public class Dispatcher : Inbox
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly int _p2pId;
        private readonly PeerBean _peerBeanMaster;
        private readonly int _heartBeatMillis;

        /* Copy on write map. The Number320 key can be divided into two parts:
         * - first Number160 is the peer ID that registers
         * - second Number160 is the peer ID for which the IO handler is registered
         * For example, a relay peer can register a handler on behalf of another peer.
         * */
        private volatile IDictionary<Number320, IDictionary<int, DispatchHandler>> _ioHandlers = new Dictionary<Number320, IDictionary<int, DispatchHandler>>();

        /// <summary>
        /// Creates a dispatcher.
        /// </summary>
        /// <param name="p2pId">The P2P ID the dispatcher is looking for incoming messages.</param>
        /// <param name="peerBeanMaster"></param>
        /// <param name="heartBeatMillis"></param>
        public Dispatcher(int p2pId, PeerBean peerBeanMaster, int heartBeatMillis)
        {
            _p2pId = p2pId;
            _peerBeanMaster = peerBeanMaster;
            _heartBeatMillis = heartBeatMillis;
        }

        /// <summary>
        /// Registers a handler with this dispatcher. Future received messages adhering to the given parameters will be
        /// forwarded to that handler. Note that the dispatcher only handles REQUEST messages. This method is thread-safe,
        /// and uses copy on write as it is expected to run this only during initialization.
        /// </summary>
        /// <param name="peerId">Specifies the receiver the dispatcher filters for. This allows to use one dispatcher for 
        /// several interfaces or even nodes.</param>
        /// <param name="onBehalfOf">The IO Handler can be registered for the own use in behalf of another peer.
        /// (E.g., in case of a relay node.)</param>
        /// <param name="ioHandler">The handler which should process the given type of messages.</param>
        /// <param name="names">The command of the Message the given handler processes. All messages having that command will
        /// be forwarded to the given handler.
        /// Note: If you register multiple handlers with the same command, only the last registered handler will receive 
        /// these messages!</param>
        public void RegisterIOHandler(Number160 peerId, Number160 onBehalfOf, DispatchHandler ioHandler,
            params int[] names)
        {
            IDictionary<Number320, IDictionary<int, DispatchHandler>> copy = new Dictionary<Number320, IDictionary<int, DispatchHandler>>(_ioHandlers);
            IDictionary<int, DispatchHandler> types;
            bool contains = copy.TryGetValue(new Number320(peerId, onBehalfOf), out types); // TODO what if not present?
            if (contains == false)
            {
                types = new Dictionary<int, DispatchHandler>();
                copy.Add(new Number320(peerId, onBehalfOf), types);
            }
            foreach (int name in names)
            {
                types.Add(name, ioHandler);
            }
            _ioHandlers = new ReadOnlyDictionary<Number320, IDictionary<int, DispatchHandler>>(copy);
        }

        /// <summary>
        /// If we shutdown, we remove the handlers. This means that a server may respond that the handler is unknown.
        /// </summary>
        /// <param name="peerId">The ID of the peer to remove the handlers.</param>
        /// <param name="onBehalfOf">The IO Handler can be registered for the own use in behalf of another peer.
        /// (E.g., in case of a relay node.)</param>
        public void RemoveIOHandlers(Number160 peerId, Number160 onBehalfOf)
        {
            IDictionary<Number320, IDictionary<int, DispatchHandler>> copy = new Dictionary<Number320, IDictionary<int, DispatchHandler>>(_ioHandlers);
            copy.Remove(new Number320(peerId, onBehalfOf));
            _ioHandlers = new ReadOnlyDictionary<Number320, IDictionary<int, DispatchHandler>>(copy);
        }

        public Message.Message MessageReceived(Message.Message message)
        {
            // server-side:
            // message comes (over network) from sender
            // -> correct DispatchHandler handles response
            throw new NotImplementedException();
        }

        public override void ExceptionCaught(Exception cause)
        {
            throw new NotImplementedException();
        }
    }
}
