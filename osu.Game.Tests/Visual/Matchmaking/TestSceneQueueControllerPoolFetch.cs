// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Matchmaking.Queue;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    /// <summary>
    /// Matchmaking, ranked play and duels all go through the multiplayer hub, so a custom server that does not implement the matchmaking
    /// methods (or is temporarily failing them) must leave the rest of the client usable rather than throwing out of <see cref="QueueController"/>.
    /// </summary>
    public partial class TestSceneQueueControllerPoolFetch : OsuTestScene
    {
        private FlakyPoolsMultiplayerClient client = null!;
        private QueueController controller = null!;

        [Test]
        public void TestUnsupportedServerYieldsNoPoolsInsteadOfThrowing()
        {
            createController(poolsSupported: false);
            AddUntilStep("pool fetch was attempted", () => client.FetchAttempts, () => Is.GreaterThan(0));

            AddAssert("pools are empty and fetching does not throw", () => tryGetPools()?.Length == 0);
        }

        [Test]
        public void TestPoolsAreFetchedAgainOnceTheServerRecovers()
        {
            createController(poolsSupported: false);
            AddUntilStep("pool fetch was attempted", () => client.FetchAttempts, () => Is.GreaterThan(0));

            AddStep("server starts supporting matchmaking", () => client.PoolsSupported = true);
            AddStep("reconnect", () =>
            {
                client.Disconnect();
                client.Connect();
            });

            AddUntilStep("pools are returned", () => tryGetPools()?.Length > 0);
        }

        [Test]
        public void TestSupportedServerReturnsPools()
        {
            createController(poolsSupported: true);

            AddUntilStep("pools are returned", () => tryGetPools()?.Length > 0);
        }

        private void createController(bool poolsSupported)
        {
            AddStep("create queue controller", () =>
            {
                client = new FlakyPoolsMultiplayerClient { PoolsSupported = poolsSupported };
                controller = new QueueController();

                Child = new DependencyProvidingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    CachedDependencies = new (Type, object)[]
                    {
                        (typeof(MultiplayerClient), client),
                        (typeof(QueueController), controller),
                    },
                    Children = new Drawable[] { client, controller },
                };
            });
        }

        /// <summary>
        /// Returns the pools reported by the controller, or <see langword="null"/> if asking for them throws.
        /// </summary>
        private MatchmakingPool[]? tryGetPools()
        {
            try
            {
                return controller.GetMatchmakingPools().GetResultSafely().ToArray();
            }
            catch
            {
                return null;
            }
        }

        private partial class FlakyPoolsMultiplayerClient : TestMultiplayerClient
        {
            public bool PoolsSupported = true;

            public int FetchAttempts { get; private set; }

            public override Task<MatchmakingPool[]> GetMatchmakingPoolsOfType(MatchmakingPoolType type)
            {
                FetchAttempts++;

                if (!PoolsSupported)
                    return Task.FromException<MatchmakingPool[]>(new HubException("Method does not exist."));

                return base.GetMatchmakingPoolsOfType(type);
            }
        }
    }
}
