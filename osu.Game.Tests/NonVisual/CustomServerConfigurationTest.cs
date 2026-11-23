// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public partial class CustomServerConfigurationTest : ImportTest
    {
        [Test]
        public void TestCustomServerSecretClearedOnStartupWhenNotRemembered()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = loadOsuWith(host, config =>
                    {
                        config.SetValue(OsuSetting.CustomServerClientSecret, "super-secret");
                        config.SetValue(OsuSetting.RememberCustomServerSecret, false);
                    });
                    var loadedConfig = osu.Dependencies.Get<OsuConfigManager>();

                    Assert.That(loadedConfig.Get<string>(OsuSetting.CustomServerClientSecret), Is.Empty);
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestCustomServerSecretPreservedOnStartupWhenRemembered()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = loadOsuWith(host, config =>
                    {
                        config.SetValue(OsuSetting.RememberCustomServerSecret, true);
                        config.SetValue(OsuSetting.CustomServerClientSecret, "super-secret");
                    });
                    var loadedConfig = osu.Dependencies.Get<OsuConfigManager>();

                    Assert.That(loadedConfig.Get<string>(OsuSetting.CustomServerClientSecret), Is.EqualTo("super-secret"));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestCustomServerEndpointsAreApplied()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = loadOsuWith(host, config =>
                    {
                        config.SetValue(OsuSetting.UseCustomServer, true);
                        config.SetValue(OsuSetting.CustomServerUrl, "https://example.test/base/");
                        config.SetValue(OsuSetting.CustomServerClientID, "12345");
                        config.SetValue(OsuSetting.CustomServerClientSecret, "abcde");
                        config.SetValue(OsuSetting.RememberCustomServerSecret, true);
                    });
                    var api = osu.Dependencies.Get<IAPIProvider>();

                    Assert.That(api, Is.TypeOf<APIAccess>());
                    Assert.That(api.Endpoints.WebsiteUrl, Is.EqualTo("https://example.test/base"));
                    Assert.That(api.Endpoints.APIUrl, Is.EqualTo("https://example.test/base"));
                    Assert.That(api.Endpoints.SpectatorUrl, Is.EqualTo("https://example.test/base/spectator"));
                    Assert.That(api.Endpoints.MultiplayerUrl, Is.EqualTo("https://example.test/base/multiplayer"));
                    Assert.That(api.Endpoints.MetadataUrl, Is.EqualTo("https://example.test/base/metadata"));
                    Assert.That(api.Endpoints.BeatmapSubmissionServiceUrl, Is.EqualTo("https://example.test/base"));
                    Assert.That(api.Endpoints.APIClientID, Is.EqualTo("12345"));
                    Assert.That(api.Endpoints.APIClientSecret, Is.EqualTo("abcde"));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestCustomSocketServerEndpointsAreApplied()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = loadOsuWith(host, config =>
                    {
                        config.SetValue(OsuSetting.UseCustomServer, true);
                        config.SetValue(OsuSetting.CustomServerUrl, "https://example.test/base/");
                        config.SetValue(OsuSetting.CustomServerSpectatorSocketUrl, "https://spectator.example.test/realtime/");
                        config.SetValue(OsuSetting.CustomServerMultiplayerSocketUrl, "https://multiplayer.example.test/play");
                        config.SetValue(OsuSetting.CustomServerMetadataSocketUrl, "https://metadata.example.test");
                        config.SetValue(OsuSetting.CustomServerClientID, "12345");
                        config.SetValue(OsuSetting.CustomServerClientSecret, "abcde");
                        config.SetValue(OsuSetting.RememberCustomServerSecret, true);
                    });
                    var api = osu.Dependencies.Get<IAPIProvider>();

                    Assert.That(api, Is.TypeOf<APIAccess>());
                    Assert.That(api.Endpoints.WebsiteUrl, Is.EqualTo("https://example.test/base"));
                    Assert.That(api.Endpoints.APIUrl, Is.EqualTo("https://example.test/base"));
                    Assert.That(api.Endpoints.SpectatorUrl, Is.EqualTo("https://spectator.example.test/realtime/spectator"));
                    Assert.That(api.Endpoints.MultiplayerUrl, Is.EqualTo("https://multiplayer.example.test/play/multiplayer"));
                    Assert.That(api.Endpoints.MetadataUrl, Is.EqualTo("https://metadata.example.test/metadata"));
                    Assert.That(api.Endpoints.BeatmapSubmissionServiceUrl, Is.EqualTo("https://example.test/base"));
                    Assert.That(api.Endpoints.APIClientID, Is.EqualTo("12345"));
                    Assert.That(api.Endpoints.APIClientSecret, Is.EqualTo("abcde"));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestMatchmakingAndDuelsUseTheCustomMultiplayerUrl()
        {
            // Quick play, ranked play and duels have no endpoint of their own: they are hub methods on the multiplayer connection.
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = loadOsuWith(host, config =>
                    {
                        config.SetValue(OsuSetting.UseCustomServer, true);
                        config.SetValue(OsuSetting.CustomServerUrl, "https://example.test/base/");
                        config.SetValue(OsuSetting.CustomServerMultiplayerSocketUrl, "https://multiplayer.example.test/play");
                    });

                    var client = osu.Dependencies.Get<MultiplayerClient>();

                    Assert.That(client, Is.TypeOf<OnlineMultiplayerClient>());
                    Assert.That(typeof(OnlineMultiplayerClient).GetField("endpoint", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(client),
                        Is.EqualTo("https://multiplayer.example.test/play/multiplayer"));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestSocketUrlsFallBackToTheServerUrlIndividually()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = loadOsuWith(host, config =>
                    {
                        config.SetValue(OsuSetting.UseCustomServer, true);
                        config.SetValue(OsuSetting.CustomServerUrl, "https://example.test/base/");
                        config.SetValue(OsuSetting.CustomServerMultiplayerSocketUrl, "https://multiplayer.example.test");
                    });
                    var api = osu.Dependencies.Get<IAPIProvider>();

                    Assert.That(api.Endpoints.SpectatorUrl, Is.EqualTo("https://example.test/base/spectator"));
                    Assert.That(api.Endpoints.MultiplayerUrl, Is.EqualTo("https://multiplayer.example.test/multiplayer"));
                    Assert.That(api.Endpoints.MetadataUrl, Is.EqualTo("https://example.test/base/metadata"));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestInvalidCustomServerFallsBackToDefaultEndpoints()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = loadOsuWith(host, config =>
                    {
                        config.SetValue(OsuSetting.UseCustomServer, true);
                        config.SetValue(OsuSetting.CustomServerUrl, "not a valid url");
                    });
                    var api = osu.Dependencies.Get<IAPIProvider>();

                    var expectedDefault = new TestOsuGameWithRealApi().CreateEndpoints();

                    Assert.That(api.Endpoints.WebsiteUrl, Is.EqualTo(expectedDefault.WebsiteUrl));
                    Assert.That(api.Endpoints.APIUrl, Is.EqualTo(expectedDefault.APIUrl));
                    Assert.That(api.Endpoints.SpectatorUrl, Is.EqualTo(expectedDefault.SpectatorUrl));
                    Assert.That(api.Endpoints.MultiplayerUrl, Is.EqualTo(expectedDefault.MultiplayerUrl));
                    Assert.That(api.Endpoints.MetadataUrl, Is.EqualTo(expectedDefault.MetadataUrl));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestEmptyCustomServerFallsBackToDefaultEndpoints()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = loadOsuWith(host, config =>
                    {
                        config.SetValue(OsuSetting.UseCustomServer, true);
                        config.SetValue(OsuSetting.CustomServerUrl, string.Empty);
                    });
                    var api = osu.Dependencies.Get<IAPIProvider>();

                    var expectedDefault = new TestOsuGameWithRealApi().CreateEndpoints();

                    Assert.That(api.Endpoints.WebsiteUrl, Is.EqualTo(expectedDefault.WebsiteUrl));
                    Assert.That(api.Endpoints.APIUrl, Is.EqualTo(expectedDefault.APIUrl));
                    Assert.That(api.Endpoints.SpectatorUrl, Is.EqualTo(expectedDefault.SpectatorUrl));
                    Assert.That(api.Endpoints.MultiplayerUrl, Is.EqualTo(expectedDefault.MultiplayerUrl));
                    Assert.That(api.Endpoints.MetadataUrl, Is.EqualTo(expectedDefault.MetadataUrl));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        [Test]
        public void TestInvalidCustomSocketServerFallsBackToDefaultEndpoints()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    var osu = loadOsuWith(host, config =>
                    {
                        config.SetValue(OsuSetting.UseCustomServer, true);
                        config.SetValue(OsuSetting.CustomServerUrl, "https://example.test/base/");
                        config.SetValue(OsuSetting.CustomServerSpectatorSocketUrl, "https://spectator.example.test/");
                        config.SetValue(OsuSetting.CustomServerMultiplayerSocketUrl, "not a valid url");
                    });
                    var api = osu.Dependencies.Get<IAPIProvider>();

                    var expectedDefault = new TestOsuGameWithRealApi().CreateEndpoints();

                    Assert.That(api.Endpoints.WebsiteUrl, Is.EqualTo(expectedDefault.WebsiteUrl));
                    Assert.That(api.Endpoints.APIUrl, Is.EqualTo(expectedDefault.APIUrl));
                    Assert.That(api.Endpoints.SpectatorUrl, Is.EqualTo(expectedDefault.SpectatorUrl));
                    Assert.That(api.Endpoints.MultiplayerUrl, Is.EqualTo(expectedDefault.MultiplayerUrl));
                    Assert.That(api.Endpoints.MetadataUrl, Is.EqualTo(expectedDefault.MetadataUrl));
                }
                finally
                {
                    host.Exit();
                }
            }
        }

        protected override TestOsuGameBase LoadOsuIntoHost(GameHost host, bool withBeatmap = false)
            => loadOsuWith(host, null, withBeatmap);

        /// <summary>
        /// Starts the game in <paramref name="host"/>, first applying <paramref name="configure"/> to the game's settings.
        /// </summary>
        /// <remarks>
        /// Settings cannot be written beforehand through the host: its storage does not exist until the host runs, and
        /// <see cref="CleanRunHeadlessGameHost"/> wipes the storage directory when it starts. The game therefore applies them
        /// itself, after the storage is ready and before it creates its configuration (see <see cref="TestOsuGameWithRealApi.SetHost"/>).
        /// </remarks>
        private TestOsuGameBase loadOsuWith(GameHost host, Action<OsuConfigManager> configure, bool withBeatmap = false)
        {
            var osu = new TestOsuGameWithRealApi(withBeatmap, configure);

            Task.Factory.StartNew(() => host.Run(osu), TaskCreationOptions.LongRunning)
                .ContinueWith(t => Assert.Fail($"Host threw exception {t.Exception}"), TaskContinuationOptions.OnlyOnFaulted);

            waitForOrAssert(() => osu.IsLoaded, @"osu! failed to start in a reasonable amount of time");

            bool ready = false;

            // wait for two update frames to be executed so that all components have had a chance to complete loading.
            host.UpdateThread.Scheduler.Add(() => host.UpdateThread.Scheduler.Add(() => ready = true));

            waitForOrAssert(() => ready, @"osu! failed to start in a reasonable amount of time");

            return osu;
        }

        private static void waitForOrAssert(Func<bool> result, string failureMessage, int timeout = 60000)
        {
            Task task = Task.Run(() =>
            {
                while (!result())
                    Thread.Sleep(200);
            });

            Assert.That(task.Wait(timeout), Is.True, failureMessage);
        }

        private partial class TestOsuGameWithRealApi : TestOsuGameBase
        {
            private readonly Action<OsuConfigManager> configure;

            public TestOsuGameWithRealApi(bool withBeatmap = false, Action<OsuConfigManager> configure = null)
                : base(withBeatmap)
            {
                this.configure = configure;

                // TestOsuGameBase installs a dummy API. These tests assert on the endpoints of the real one, so clear it and let
                // OsuGameBase create an APIAccess. (The property is protected and hidden by TestOsuGameBase, hence reflection.)
                typeof(OsuGameBase).GetProperty(nameof(API), BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(this, null);
            }

            public override void SetHost(GameHost host)
            {
                // OsuGameBase.SetHost creates the config manager from this storage, so the settings have to be in place before it.
                // It must be the same kind the game will create: development builds read "game.dev.ini", not "game.ini".
                if (configure != null)
                {
                    using (var config = UseDevelopmentServer ? new DevelopmentOsuConfigManager(host.Storage) : new OsuConfigManager(host.Storage))
                        configure(config);
                }

                base.SetHost(host);
            }
        }
    }
}
