// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Online.API;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class CustomServerConfigurationTest : ImportTest
    {
        [Test]
        public void TestCustomServerSecretClearedOnStartupWhenNotRemembered()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    using (var config = new OsuConfigManager(host.Storage))
                    {
                        config.SetValue(OsuSetting.CustomServerClientSecret, "super-secret");
                        config.SetValue(OsuSetting.RememberCustomServerSecret, false);
                    }

                    var osu = LoadOsuIntoHost(host);
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
                    using (var config = new OsuConfigManager(host.Storage))
                    {
                        config.SetValue(OsuSetting.RememberCustomServerSecret, true);
                        config.SetValue(OsuSetting.CustomServerClientSecret, "super-secret");
                    }

                    var osu = LoadOsuIntoHost(host);
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
                    using (var config = new OsuConfigManager(host.Storage))
                    {
                        config.SetValue(OsuSetting.UseCustomServer, true);
                        config.SetValue(OsuSetting.CustomServerUrl, "https://example.test/base/");
                        config.SetValue(OsuSetting.CustomServerSocketUrl, string.Empty);
                        config.SetValue(OsuSetting.CustomServerClientID, "12345");
                        config.SetValue(OsuSetting.CustomServerClientSecret, "abcde");
                        config.SetValue(OsuSetting.RememberCustomServerSecret, true);
                    }

                    var osu = LoadOsuIntoHost(host);
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
                    using (var config = new OsuConfigManager(host.Storage))
                    {
                        config.SetValue(OsuSetting.UseCustomServer, true);
                        config.SetValue(OsuSetting.CustomServerUrl, "https://example.test/base/");
                        config.SetValue(OsuSetting.CustomServerSocketUrl, "https://socket.example.test/realtime/");
                        config.SetValue(OsuSetting.CustomServerClientID, "12345");
                        config.SetValue(OsuSetting.CustomServerClientSecret, "abcde");
                        config.SetValue(OsuSetting.RememberCustomServerSecret, true);
                    }

                    var osu = LoadOsuIntoHost(host);
                    var api = osu.Dependencies.Get<IAPIProvider>();

                    Assert.That(api, Is.TypeOf<APIAccess>());
                    Assert.That(api.Endpoints.WebsiteUrl, Is.EqualTo("https://example.test/base"));
                    Assert.That(api.Endpoints.APIUrl, Is.EqualTo("https://example.test/base"));
                    Assert.That(api.Endpoints.SpectatorUrl, Is.EqualTo("https://socket.example.test/realtime/spectator"));
                    Assert.That(api.Endpoints.MultiplayerUrl, Is.EqualTo("https://socket.example.test/realtime/multiplayer"));
                    Assert.That(api.Endpoints.MetadataUrl, Is.EqualTo("https://socket.example.test/realtime/metadata"));
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
        public void TestInvalidCustomServerFallsBackToDefaultEndpoints()
        {
            using (HeadlessGameHost host = new CleanRunHeadlessGameHost())
            {
                try
                {
                    using (var config = new OsuConfigManager(host.Storage))
                    {
                        config.SetValue(OsuSetting.UseCustomServer, true);
                        config.SetValue(OsuSetting.CustomServerUrl, "not a valid url");
                    }

                    var osu = LoadOsuIntoHost(host);
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
                    using (var config = new OsuConfigManager(host.Storage))
                    {
                        config.SetValue(OsuSetting.UseCustomServer, true);
                        config.SetValue(OsuSetting.CustomServerUrl, string.Empty);
                    }

                    var osu = LoadOsuIntoHost(host);
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
                    using (var config = new OsuConfigManager(host.Storage))
                    {
                        config.SetValue(OsuSetting.UseCustomServer, true);
                        config.SetValue(OsuSetting.CustomServerUrl, "https://example.test/base/");
                        config.SetValue(OsuSetting.CustomServerSocketUrl, "not a valid url");
                    }

                    var osu = LoadOsuIntoHost(host);
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
        {
            var osu = new TestOsuGameWithRealApi(withBeatmap);

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
            public TestOsuGameWithRealApi(bool withBeatmap = false)
                : base(withBeatmap)
            {
                API = null;
            }
        }
    }
}
