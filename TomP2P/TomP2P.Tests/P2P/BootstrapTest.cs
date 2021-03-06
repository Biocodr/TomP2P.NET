﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using TomP2P.Core.P2P;
using TomP2P.Core.Peers;
using TomP2P.Extensions;

namespace TomP2P.Tests.P2P
{
    [TestFixture]
    public class BootstrapTest
    {
        [Test]
        public async void TestBootstrapDiscover()
        {
            var rnd = new Random(42);
            Peer master = null;
            Peer slave = null;
            try
            {
                master = new PeerBuilder(new Number160(rnd))
                    .SetMaintenanceTask(Utils2.CreateInfiniteIntervalMaintenanceTask())
                    .SetChannelServerConfiguration(Utils2.CreateInfiniteTimeoutChannelServerConfiguration(4001, 4001))
                    .SetPorts(4001)
                    .Start();
                slave = new PeerBuilder(new Number160(rnd))
                    .SetMaintenanceTask(Utils2.CreateInfiniteIntervalMaintenanceTask())
                    .SetChannelServerConfiguration(Utils2.CreateInfiniteTimeoutChannelServerConfiguration(4002, 4002))
                    .SetPorts(4002)
                    .Start();

                var tcsDiscover = master.Discover().SetPeerAddress(slave.PeerAddress).Start();
                await tcsDiscover.Task;

                Assert.IsTrue(!tcsDiscover.Task.IsFaulted);
            }
            finally
            {
                if (master != null)
                {
                    master.ShutdownAsync().Wait();
                }
                if (slave != null)
                {
                    slave.ShutdownAsync().Wait();
                }
            }
        }

        [Test]
        public async void TestBootstrapFail()
        {
            var rnd = new Random(42);
            Peer master = null;
            Peer slave = null;
            try
            {
                master = new PeerBuilder(new Number160(rnd))
                    .SetMaintenanceTask(Utils2.CreateInfiniteIntervalMaintenanceTask())
                    .SetChannelServerConfiguration(Utils2.CreateInfiniteTimeoutChannelServerConfiguration(4001, 4001))
                    .SetPorts(4001)
                    .Start();
                slave = new PeerBuilder(new Number160(rnd))
                    .SetMaintenanceTask(Utils2.CreateInfiniteIntervalMaintenanceTask())
                    .SetChannelServerConfiguration(Utils2.CreateInfiniteTimeoutChannelServerConfiguration(4002, 4002))
                    .SetPorts(4002)
                    .Start();

                // bootstrap to another address/port
                var taskBootstrap = master.Bootstrap()
                    .SetInetAddress(IPAddress.Loopback)
                    .SetPorts(3000)
                    .StartAsync();
                try
                {
                    await taskBootstrap;
                    Assert.Fail("The bootstrapping task should fail.");
                }
                catch (Exception)
                {
                    Assert.IsTrue(taskBootstrap.IsFaulted);
                }

                // bootstrap to correct address
                taskBootstrap = master.Bootstrap()
                    .SetPeerAddress(slave.PeerAddress)
                    .StartAsync();
                await taskBootstrap;
                Assert.IsTrue(!taskBootstrap.IsFaulted);
            }
            finally
            {
                if (master != null)
                {
                    master.ShutdownAsync().Wait();
                }
                if (slave != null)
                {
                    slave.ShutdownAsync().Wait();
                }
            }
        }

        [Test]
        public async void TestBootstrap()
        {
            var rnd = new Random(42);
            Peer master = null;
            try
            {
                // setup
                // TODO test with >100 peers
                // TODO test with timeout
                var peers = Utils2.CreateNodes(200, rnd, 4004);
                master = peers[0];

                // do testing
                var list = new List<Task>();
                for (int i = 0; i < peers.Length; i++)
                {
                    if (peers[i] != master)
                    {
                        Console.WriteLine("Bootstrapping peer {0}.", i);
                        var taskBootstrap = peers[i].Bootstrap().SetPeerAddress(master.PeerAddress).StartAsync();
                        list.Add(taskBootstrap);
                    }
                }
                foreach (var bootstrapTask in list)
                {
                    try
                    {
                        await bootstrapTask;
                    }
                    catch (Exception)
                    {
                        Console.Error.WriteLine(bootstrapTask.TryGetException());
                    }
                    finally
                    {
                        Assert.IsTrue(!bootstrapTask.IsFaulted);
                    }
                }
            }
            finally
            {
                if (master != null)
                {
                    master.ShutdownAsync().Wait();
                }
            }
        }
    }
}
