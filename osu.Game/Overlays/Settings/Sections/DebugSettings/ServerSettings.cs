// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.DebugSettings
{
    public partial class ServerSettings : SettingsSubsection
    {
        protected override LocalisableString Header => DebugSettingsStrings.ServerHeader;

        private FormCheckBox useCustomServer = null!;
        private FormTextBox customServerUrl = null!;
        private FormTextBox customServerSpectatorSocketUrl = null!;
        private FormTextBox customServerMultiplayerSocketUrl = null!;
        private FormTextBox customServerMetadataSocketUrl = null!;
        private FormTextBox customServerClientID = null!;
        private FormSecretTextBox customServerClientSecret = null!;
        private FormCheckBox rememberCustomServerSecret = null!;
        private SettingsButtonV2 resetToDefaultServerButton = null!;
        private SettingsButtonV2 restartToApplyServerChangesButton = null!;
        private SettingsNote note = null!;

        private IBindable<APIState> apiState = null!;

        private IBindable<bool> customServerEnabledBindable = null!;
        private IBindable<string> customServerUrlBindable = null!;
        private IBindable<string> customServerSpectatorSocketUrlBindable = null!;
        private IBindable<string> customServerMultiplayerSocketUrlBindable = null!;
        private IBindable<string> customServerMetadataSocketUrlBindable = null!;
        private IBindable<string> customServerClientIDBindable = null!;
        private IBindable<string> customServerClientSecretBindable = null!;
        private IBindable<bool> rememberCustomServerSecretBindable = null!;

        private ServerSettingsSnapshot initialSnapshot;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider api, OsuGameBase? game, IDialogOverlay? dialogOverlay)
        {
            customServerEnabledBindable = config.GetBindable<bool>(OsuSetting.UseCustomServer);
            customServerUrlBindable = config.GetBindable<string>(OsuSetting.CustomServerUrl);
            customServerSpectatorSocketUrlBindable = config.GetBindable<string>(OsuSetting.CustomServerSpectatorSocketUrl);
            customServerMultiplayerSocketUrlBindable = config.GetBindable<string>(OsuSetting.CustomServerMultiplayerSocketUrl);
            customServerMetadataSocketUrlBindable = config.GetBindable<string>(OsuSetting.CustomServerMetadataSocketUrl);
            customServerClientIDBindable = config.GetBindable<string>(OsuSetting.CustomServerClientID);
            customServerClientSecretBindable = config.GetBindable<string>(OsuSetting.CustomServerClientSecret);
            rememberCustomServerSecretBindable = config.GetBindable<bool>(OsuSetting.RememberCustomServerSecret);

            apiState = api.State.GetBoundCopy();

            Children = new Drawable[]
            {
                new SettingsItemV2(useCustomServer = new FormCheckBox
                {
                    Caption = DebugSettingsStrings.UseCustomServer,
                    Current = { BindTarget = customServerEnabledBindable },
                }),
                new SettingsItemV2(customServerUrl = new FormTextBox
                {
                    Caption = DebugSettingsStrings.ServerUrl,
                    PlaceholderText = @"https://localhost:8000",
                    Current = { BindTarget = customServerUrlBindable },
                }),
                new SettingsItemV2(customServerSpectatorSocketUrl = new FormTextBox
                {
                    Caption = DebugSettingsStrings.SpectatorSocketServerUrl,
                    HintText = DebugSettingsStrings.SocketServerUrlHint,
                    PlaceholderText = @"https://localhost:8000",
                    Current = { BindTarget = customServerSpectatorSocketUrlBindable },
                }),
                new SettingsItemV2(customServerMultiplayerSocketUrl = new FormTextBox
                {
                    Caption = DebugSettingsStrings.MultiplayerSocketServerUrl,
                    HintText = DebugSettingsStrings.SocketServerUrlHint,
                    PlaceholderText = @"https://localhost:8000",
                    Current = { BindTarget = customServerMultiplayerSocketUrlBindable },
                }),
                new SettingsItemV2(customServerMetadataSocketUrl = new FormTextBox
                {
                    Caption = DebugSettingsStrings.MetadataSocketServerUrl,
                    HintText = DebugSettingsStrings.SocketServerUrlHint,
                    PlaceholderText = @"https://localhost:8000",
                    Current = { BindTarget = customServerMetadataSocketUrlBindable },
                }),
                new SettingsItemV2(customServerClientID = new FormTextBox
                {
                    Caption = DebugSettingsStrings.ClientId,
                    Current = { BindTarget = customServerClientIDBindable },
                }),
                new SettingsItemV2(customServerClientSecret = new FormSecretTextBox
                {
                    Caption = DebugSettingsStrings.ClientSecret,
                    Current = { BindTarget = customServerClientSecretBindable },
                }),
                new SettingsItemV2(rememberCustomServerSecret = new FormCheckBox
                {
                    Caption = DebugSettingsStrings.RememberClientSecret,
                    Current = { BindTarget = rememberCustomServerSecretBindable },
                }),
                resetToDefaultServerButton = new SettingsButtonV2
                {
                    Text = DebugSettingsStrings.UseDefaultServer,
                    Action = () => useCustomServer.Current.Value = false,
                },
                restartToApplyServerChangesButton = new SettingsButtonV2
                {
                    Text = DebugSettingsStrings.RestartToApplyServerChanges,
                    Action = () => restartToApplyServerChanges(game, dialogOverlay),
                },
                note = new SettingsNote()
            };

            initialSnapshot = createSnapshot();

            customServerEnabledBindable.BindValueChanged(_ => Scheduler.AddOnce(updateState));
            customServerUrlBindable.BindValueChanged(_ => Scheduler.AddOnce(updateState));
            customServerSpectatorSocketUrlBindable.BindValueChanged(_ => Scheduler.AddOnce(updateState));
            customServerMultiplayerSocketUrlBindable.BindValueChanged(_ => Scheduler.AddOnce(updateState));
            customServerMetadataSocketUrlBindable.BindValueChanged(_ => Scheduler.AddOnce(updateState));
            customServerClientIDBindable.BindValueChanged(_ => Scheduler.AddOnce(updateState));
            customServerClientSecretBindable.BindValueChanged(_ => Scheduler.AddOnce(updateState));
            rememberCustomServerSecretBindable.BindValueChanged(_ => Scheduler.AddOnce(updateState));
            apiState.BindValueChanged(_ => Scheduler.AddOnce(updateState), true);
        }

        private void updateState()
        {
            bool canEdit = apiState.Value == APIState.Offline;
            bool customServerEnabled = useCustomServer.Current.Value;
            bool hasPendingChanges = hasPendingServerChanges();
            bool hasValidServerUrl = !customServerEnabled || tryNormaliseServerUrl(customServerUrl.Current.Value, out _);
            bool hasValidSpectatorSocketServerUrl = !customServerEnabled
                                                    || string.IsNullOrWhiteSpace(customServerSpectatorSocketUrl.Current.Value)
                                                    || tryNormaliseServerUrl(customServerSpectatorSocketUrl.Current.Value, out _);
            bool hasValidMultiplayerSocketServerUrl = !customServerEnabled
                                                      || string.IsNullOrWhiteSpace(customServerMultiplayerSocketUrl.Current.Value)
                                                      || tryNormaliseServerUrl(customServerMultiplayerSocketUrl.Current.Value, out _);
            bool hasValidMetadataSocketServerUrl = !customServerEnabled
                                                   || string.IsNullOrWhiteSpace(customServerMetadataSocketUrl.Current.Value)
                                                   || tryNormaliseServerUrl(customServerMetadataSocketUrl.Current.Value, out _);
            bool hasValidSocketServerUrls = hasValidSpectatorSocketServerUrl && hasValidMultiplayerSocketServerUrl && hasValidMetadataSocketServerUrl;

            useCustomServer.Current.Disabled = !canEdit;
            // Keep text bindables writable to avoid TextBox load synchronisation exceptions.
            customServerUrl.ReadOnly = !canEdit || !customServerEnabled;
            customServerSpectatorSocketUrl.ReadOnly = !canEdit || !customServerEnabled;
            customServerMultiplayerSocketUrl.ReadOnly = !canEdit || !customServerEnabled;
            customServerMetadataSocketUrl.ReadOnly = !canEdit || !customServerEnabled;
            customServerClientID.ReadOnly = !canEdit || !customServerEnabled;
            customServerClientSecret.ReadOnly = !canEdit || !customServerEnabled;
            rememberCustomServerSecret.Current.Disabled = !canEdit || !customServerEnabled;

            bool canRevertToDefault = canEdit && customServerEnabled;

            resetToDefaultServerButton.Enabled.Value = canRevertToDefault;
            restartToApplyServerChangesButton.Enabled.Value = canEdit && hasPendingChanges && hasValidServerUrl && hasValidSocketServerUrls;

            note.Current.Value = createNote(canEdit, customServerEnabled, hasPendingChanges, hasValidServerUrl, hasValidSpectatorSocketServerUrl, hasValidMultiplayerSocketServerUrl, hasValidMetadataSocketServerUrl);
        }

        private SettingsNote.Data? createNote(bool canEdit, bool customServerEnabled, bool hasPendingChanges, bool hasValidServerUrl, bool hasValidSpectatorSocketServerUrl, bool hasValidMultiplayerSocketServerUrl, bool hasValidMetadataSocketServerUrl)
        {
            if (!canEdit)
                return new SettingsNote.Data(DebugSettingsStrings.SignOutBeforeChangingServer, SettingsNote.Type.Critical);

            if (customServerEnabled && !hasValidServerUrl)
                return new SettingsNote.Data(DebugSettingsStrings.InvalidServerUrl, SettingsNote.Type.Critical);

            if (customServerEnabled && !hasValidSpectatorSocketServerUrl)
                return new SettingsNote.Data(DebugSettingsStrings.InvalidSpectatorSocketServerUrl, SettingsNote.Type.Critical);

            if (customServerEnabled && !hasValidMultiplayerSocketServerUrl)
                return new SettingsNote.Data(DebugSettingsStrings.InvalidMultiplayerSocketServerUrl, SettingsNote.Type.Critical);

            if (customServerEnabled && !hasValidMetadataSocketServerUrl)
                return new SettingsNote.Data(DebugSettingsStrings.InvalidMetadataSocketServerUrl, SettingsNote.Type.Critical);

            if (hasPendingChanges)
                return new SettingsNote.Data(DebugSettingsStrings.RestartRequiredToApplyServerChanges, SettingsNote.Type.Warning);

            if (customServerEnabled && rememberCustomServerSecret.Current.Value)
                return new SettingsNote.Data(DebugSettingsStrings.ClientSecretStorageWarning, SettingsNote.Type.Informational);

            return null;
        }

        private bool hasPendingServerChanges() => createSnapshot() != initialSnapshot;

        private ServerSettingsSnapshot createSnapshot() => new ServerSettingsSnapshot(
            useCustomServer.Current.Value,
            customServerUrl.Current.Value,
            customServerSpectatorSocketUrl.Current.Value,
            customServerMultiplayerSocketUrl.Current.Value,
            customServerMetadataSocketUrl.Current.Value,
            customServerClientID.Current.Value,
            customServerClientSecret.Current.Value,
            rememberCustomServerSecret.Current.Value
        );

        private void restartToApplyServerChanges(OsuGameBase? game, IDialogOverlay? dialogOverlay)
        {
            if (game == null || !hasPendingServerChanges())
                return;

            void restart()
            {
                game.RestartAppWhenExited();
                game.AttemptExit();
            }

            if (dialogOverlay == null)
            {
                restart();
                return;
            }

            dialogOverlay.Push(new ConfirmDialog(DebugSettingsStrings.RestartRequiredForServerChangeConfirmation, restart, () => { }));
        }

        private static bool tryNormaliseServerUrl(string rawServerUrl, out string normalisedServerUrl)
        {
            normalisedServerUrl = string.Empty;

            if (string.IsNullOrWhiteSpace(rawServerUrl))
                return false;

            string candidate = rawServerUrl.Trim().TrimEnd('/');

            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
                return false;

            if (!uri.IsAbsoluteUri || string.IsNullOrEmpty(uri.Host))
                return false;

            normalisedServerUrl = uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
            return true;
        }

        private readonly record struct ServerSettingsSnapshot(
            bool UseCustomServer,
            string CustomServerUrl,
            string CustomServerSpectatorSocketUrl,
            string CustomServerMultiplayerSocketUrl,
            string CustomServerMetadataSocketUrl,
            string CustomServerClientID,
            string CustomServerClientSecret,
            bool RememberCustomServerSecret
        );

        private partial class FormSecretTextBox : FormTextBox
        {
            internal override InnerTextBox CreateTextBox() => new SecretTextBox();

            private partial class SecretTextBox : InnerTextBox
            {
                public SecretTextBox()
                {
                    InputProperties = new TextInputProperties(TextInputType.Password, false);
                }

                protected override Drawable GetDrawableCharacter(char c) => new OsuSpriteText
                {
                    Text = "*",
                };
            }
        }
    }
}
