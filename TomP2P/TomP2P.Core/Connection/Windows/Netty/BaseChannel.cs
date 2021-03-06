﻿using System;
using System.Net;
using System.Threading;
using NLog;

namespace TomP2P.Core.Connection.Windows.Netty
{
    public abstract class BaseChannel : IChannel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public event ChannelEventHandler Closed;
        public event ChannelEventHandler WriteCompleted;

        private volatile bool _isClosed;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        protected BaseChannel(IPEndPoint localEndPoint, Pipeline pipeline)
        {
            LocalEndPoint = localEndPoint;
            Pipeline = pipeline;
        }

        /// <summary>
        /// Closes the channel and notfies the subscribers of the "Closed" events.
        /// </summary>
        public void Close()
        {
            if (!_isClosed)
            {
                _isClosed = true;
                _cts.Cancel();
                try
                {
                    DoClose();
                    Logger.Debug("Closed {0}.", this);
                    NotifyClosed();
                }
                catch (ObjectDisposedException)
                {
                    // the socket seems to be disposed already
                    Logger.Error("{0} was already closed/disposed.", this);
                }
            }
        }

        protected abstract void DoClose();

        protected void NotifyClosed()
        {
            if (Closed != null)
            {
                Closed(this);
            }
        }

        /// <summary>
        /// This should be called by all deriving types upon sending finished.
        /// </summary>
        protected void NotifyWriteCompleted()
        {
            if (WriteCompleted != null)
            {
                WriteCompleted(this);
            }
        }

        public IPEndPoint LocalEndPoint { get; protected set; }

        public IPEndPoint RemoteEndPoint { get; protected set; }

        public Pipeline Pipeline { get; private set; }

        public abstract bool IsUdp { get; }

        public abstract bool IsTcp { get; }

        public bool IsOpen
        {
            get { return !_isClosed; } // TODO ok? what about if channel creation failed?
        }

        public bool IsActive
        {
            get { return !_isClosed; } // TODO ok? what about if channel creation failed?
        }

        protected bool IsClosed
        {
            get { return _isClosed; }
        }

        protected CancellationToken CloseToken
        {
            get { return _cts.Token; }
        }
    }
}
