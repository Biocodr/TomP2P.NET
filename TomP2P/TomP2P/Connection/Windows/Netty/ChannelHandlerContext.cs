﻿using System;

namespace TomP2P.Connection.Windows.Netty
{
    // TODO use exactly same structure as Java Netty --> e.g., "write" instead of "fireWrite"
    // TODO currently, this class only acts as man-in-the-middle

    /// <summary>
    /// Equivalent to Java Netty's ChannelHandlerContext.
    /// In .NET, this context is implemented with only the functionality required for this project.
    /// One context object is associated per pipeline session.
    /// </summary>
    public class ChannelHandlerContext : DefaultAttributeMap
    {
        private readonly Pipeline.PipelineSession _session;

        /// <summary>
        /// Creates a context object for a specific channel and its pipeline.
        /// </summary>
        /// <param name="session"></param>
        public ChannelHandlerContext(Pipeline.PipelineSession session)
        {
            _session = session;
        }

        /// <summary>
        /// Results in having the next outbound handler write the provided message.
        /// </summary>
        /// <param name="msg"></param>
        public void FireWrite(object msg)
        {
            // forward to pipeline
            _session.Write(msg);
        }

        /// <summary>
        /// Results in having the next inbound handler read the provided message.
        /// </summary>
        /// <param name="msg"></param>
        public void FireRead(object msg)
        {
            // forward to pipeline
            _session.Read(msg);
        }

        /// <summary>
        /// A channel encountered an exception.
        /// </summary>
        /// <param name="ex"></param>
        public void FireExceptionCaught(Exception ex)
        {
            _session.ExceptionCaught(ex);
        }

        /// <summary>
        /// A channel received a user-defined event.
        /// </summary>
        /// <param name="evt"></param>
        public void FireUserEventTriggered(object evt)
        {
            _session.UserEventTriggered(evt);
        }

        public void Close()
        {
            Channel.Close();
        }

        public IChannel Channel
        {
            get { return _session.Pipeline.Channel; }
        }
    }
}
