// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class DebugSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.DebugSettings";

        /// <summary>
        /// "Import files"
        /// </summary>
        public static LocalisableString ImportFiles => new TranslatableString(getKey(@"import_files"), @"Import files");

        /// <summary>
        /// "Run latency certifier"
        /// </summary>
        public static LocalisableString RunLatencyCertifier => new TranslatableString(getKey(@"run_latency_certifier"), @"Run latency certifier");

        /// <summary>
        /// "Server"
        /// </summary>
        public static LocalisableString ServerHeader => new TranslatableString(getKey(@"server_header"), @"Server");

        /// <summary>
        /// "Use custom server"
        /// </summary>
        public static LocalisableString UseCustomServer => new TranslatableString(getKey(@"use_custom_server"), @"Use custom server");

        /// <summary>
        /// "Server URL"
        /// </summary>
        public static LocalisableString ServerUrl => new TranslatableString(getKey(@"server_url"), @"Server URL");

        /// <summary>
        /// "Socket server URL (optional)"
        /// </summary>
        public static LocalisableString SocketServerUrl => new TranslatableString(getKey(@"socket_server_url"), @"Socket server URL (optional)");

        /// <summary>
        /// "Spectator socket server URL (optional)"
        /// </summary>
        public static LocalisableString SpectatorSocketServerUrl => new TranslatableString(getKey(@"spectator_socket_server_url"), @"Spectator socket server URL (optional)");

        /// <summary>
        /// "Multiplayer socket server URL (optional)"
        /// </summary>
        public static LocalisableString MultiplayerSocketServerUrl => new TranslatableString(getKey(@"multiplayer_socket_server_url"), @"Multiplayer socket server URL (optional)");

        /// <summary>
        /// "Metadata socket server URL (optional)"
        /// </summary>
        public static LocalisableString MetadataSocketServerUrl => new TranslatableString(getKey(@"metadata_socket_server_url"), @"Metadata socket server URL (optional)");

        /// <summary>
        /// "Leave empty to use Server URL."
        /// </summary>
        public static LocalisableString SocketServerUrlHint => new TranslatableString(getKey(@"socket_server_url_hint"), @"Leave empty to use Server URL.");

        /// <summary>
        /// "Client ID"
        /// </summary>
        public static LocalisableString ClientId => new TranslatableString(getKey(@"client_id"), @"Client ID");

        /// <summary>
        /// "Client secret"
        /// </summary>
        public static LocalisableString ClientSecret => new TranslatableString(getKey(@"client_secret"), @"Client secret");

        /// <summary>
        /// "Remember client secret"
        /// </summary>
        public static LocalisableString RememberClientSecret => new TranslatableString(getKey(@"remember_client_secret"), @"Remember client secret");

        /// <summary>
        /// "Use default server"
        /// </summary>
        public static LocalisableString UseDefaultServer => new TranslatableString(getKey(@"use_default_server"), @"Use default server");

        /// <summary>
        /// "Restart to apply server changes"
        /// </summary>
        public static LocalisableString RestartToApplyServerChanges => new TranslatableString(getKey(@"restart_to_apply_server_changes"), @"Restart to apply server changes");

        /// <summary>
        /// "Sign out before changing server settings."
        /// </summary>
        public static LocalisableString SignOutBeforeChangingServer => new TranslatableString(getKey(@"sign_out_before_changing_server"), @"Sign out before changing server settings.");

        /// <summary>
        /// "Restart is required to apply server changes."
        /// </summary>
        public static LocalisableString RestartRequiredToApplyServerChanges => new TranslatableString(getKey(@"restart_required_to_apply_server_changes"), @"Restart is required to apply server changes.");

        /// <summary>
        /// "Please enter a valid absolute server URL."
        /// </summary>
        public static LocalisableString InvalidServerUrl => new TranslatableString(getKey(@"invalid_server_url"), @"Please enter a valid absolute server URL.");

        /// <summary>
        /// "Please enter a valid absolute socket server URL."
        /// </summary>
        public static LocalisableString InvalidSocketServerUrl => new TranslatableString(getKey(@"invalid_socket_server_url"), @"Please enter a valid absolute socket server URL.");

        /// <summary>
        /// "Please enter a valid absolute spectator socket server URL."
        /// </summary>
        public static LocalisableString InvalidSpectatorSocketServerUrl => new TranslatableString(getKey(@"invalid_spectator_socket_server_url"), @"Please enter a valid absolute spectator socket server URL.");

        /// <summary>
        /// "Please enter a valid absolute multiplayer socket server URL."
        /// </summary>
        public static LocalisableString InvalidMultiplayerSocketServerUrl => new TranslatableString(getKey(@"invalid_multiplayer_socket_server_url"), @"Please enter a valid absolute multiplayer socket server URL.");

        /// <summary>
        /// "Please enter a valid absolute metadata socket server URL."
        /// </summary>
        public static LocalisableString InvalidMetadataSocketServerUrl => new TranslatableString(getKey(@"invalid_metadata_socket_server_url"), @"Please enter a valid absolute metadata socket server URL.");

        /// <summary>
        /// "Client secrets are stored in plain text while remembered."
        /// </summary>
        public static LocalisableString ClientSecretStorageWarning => new TranslatableString(getKey(@"client_secret_storage_warning"), @"Client secrets are stored in plain text while remembered.");

        /// <summary>
        /// "To apply server changes, osu! will close. Please open it again."
        /// </summary>
        public static LocalisableString RestartRequiredForServerChangeConfirmation => new TranslatableString(getKey(@"restart_required_for_server_change_confirmation"), @"To apply server changes, osu! will close. Please open it again.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
