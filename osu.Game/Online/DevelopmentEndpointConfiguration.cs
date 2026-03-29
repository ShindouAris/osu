// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online
{
    public class DevelopmentEndpointConfiguration : EndpointConfiguration
    {
        public DevelopmentEndpointConfiguration()
        {
            WebsiteUrl = APIUrl = @"https://localhost:8000";
            APIClientSecret = @"4BcTIWBAG31ASGx4dbMYwl7wZZcaQE2F2bqVGCZ1";
            APIClientID = "5";
            SpectatorUrl = "https://localhost:8000/spectator";
            MultiplayerUrl = "https://localhost:8000/multiplayer";
            MetadataUrl = "https://localhost:8000/metadata";
        }
    }
}
