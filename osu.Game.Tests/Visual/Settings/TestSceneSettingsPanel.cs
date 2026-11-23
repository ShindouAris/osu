// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections;
using osu.Game.Overlays.Settings.Sections.DebugSettings;
using osu.Game.Overlays.Settings.Sections.Input;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public partial class TestSceneSettingsPanel : OsuManualInputManagerTestScene
    {
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private SettingsPanel settings;
        private DialogOverlay dialogOverlay;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create settings", () =>
            {
                settings?.Expire();

                Add(settings = new SettingsOverlay
                {
                    State = { Value = Visibility.Visible }
                });
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("do nothing", () => { });
            AddToggleStep("toggle visibility", visible => settings.State.Value = visible ? Visibility.Visible : Visibility.Hidden);
        }

        [Test]
        public void TestFiltering([Values] bool beforeLoad)
        {
            AddStep("reset mouse", () => InputManager.MoveMouseTo(settings));

            if (beforeLoad)
                AddStep("set filter", () => settings.SectionsContainer.ChildrenOfType<SettingsSearchTextBox>().First().Current.Value = "scaling");

            AddUntilStep("wait for items to load", () => settings.SectionsContainer.ChildrenOfType<IFilterable>().Any());

            if (!beforeLoad)
                AddStep("set filter", () => settings.SectionsContainer.ChildrenOfType<SettingsSearchTextBox>().First().Current.Value = "scaling");

            AddAssert("ensure all items match filter", () => settings.SectionsContainer
                                                                     .ChildrenOfType<SettingsSection>().Where(f => f.IsPresent)
                                                                     .All(section =>
                                                                         section.Children.Where(f => f.IsPresent)
                                                                                .OfType<ISettingsItem>()
                                                                                .OfType<IFilterable>()
                                                                                .All(f => f.FilterTerms.Any(t => t.ToString().Contains("scaling")))
                                                                     ));

            AddAssert("ensure section is current", () => settings.CurrentSection.Value is GraphicsSection);
            AddAssert("ensure section is placed first", () => settings.CurrentSection.Value.Y == 0);
        }

        [Test]
        public void TestFilterAfterLoad()
        {
            AddStep("reset mouse", () => InputManager.MoveMouseTo(settings));

            AddUntilStep("wait for items to load", () => settings.SectionsContainer.ChildrenOfType<IFilterable>().Any());

            AddStep("set filter", () => settings.SectionsContainer.ChildrenOfType<SettingsSearchTextBox>().First().Current.Value = "scaling");
        }

        [Test]
        public void ToggleVisibility()
        {
            AddStep("reset mouse", () => InputManager.MoveMouseTo(settings));

            AddWaitStep("wait some", 5);
            AddToggleStep("toggle visibility", _ => settings.ToggleVisibility());
        }

        [Test]
        public void TestTextboxFocusAfterNestedPanelBackButton()
        {
            AddStep("reset mouse", () => InputManager.MoveMouseTo(settings));

            AddUntilStep("sections loaded", () => settings.SectionsContainer.Children.Count > 0);
            AddUntilStep("top-level textbox focused", () => settings.SectionsContainer.ChildrenOfType<SettingsSearchTextBox>().FirstOrDefault()?.HasFocus == true);

            AddStep("open key binding subpanel", () =>
            {
                settings.SectionsContainer
                        .ChildrenOfType<InputSection>().FirstOrDefault()?
                        .ChildrenOfType<OsuButton>().FirstOrDefault()?
                        .TriggerClick();
            });

            AddUntilStep("binding panel textbox focused", () => settings
                                                                .ChildrenOfType<KeyBindingPanel>().FirstOrDefault()?
                                                                .ChildrenOfType<SettingsSearchTextBox>().FirstOrDefault()?.HasFocus == true);

            AddStep("Press back", () => settings
                                        .ChildrenOfType<KeyBindingPanel>().FirstOrDefault()?
                                        .ChildrenOfType<SettingsSidebar.BackButton>().FirstOrDefault()?.TriggerClick());

            AddUntilStep("top-level textbox focused", () => settings.SectionsContainer.ChildrenOfType<SettingsSearchTextBox>().FirstOrDefault()?.HasFocus == true);
        }

        [Test]
        public void TestTextboxFocusAfterNestedPanelEscape()
        {
            AddStep("reset mouse", () => InputManager.MoveMouseTo(settings));

            AddUntilStep("sections loaded", () => settings.SectionsContainer.Children.Count > 0);
            AddUntilStep("top-level textbox focused", () => settings.SectionsContainer.ChildrenOfType<SettingsSearchTextBox>().FirstOrDefault()?.HasFocus == true);

            AddStep("open key binding subpanel", () =>
            {
                settings.SectionsContainer
                        .ChildrenOfType<InputSection>().FirstOrDefault()?
                        .ChildrenOfType<OsuButton>().FirstOrDefault()?
                        .TriggerClick();
            });

            AddUntilStep("binding panel textbox focused", () => settings
                                                                .ChildrenOfType<KeyBindingPanel>().FirstOrDefault()?
                                                                .ChildrenOfType<SettingsSearchTextBox>().FirstOrDefault()?.HasFocus == true);

            AddStep("Escape", () => InputManager.Key(Key.Escape));

            AddUntilStep("top-level textbox focused", () => settings.SectionsContainer.ChildrenOfType<SettingsSearchTextBox>().FirstOrDefault()?.HasFocus == true);
        }

        [Test]
        public void TestSearchTextBoxSelectedOnShow()
        {
            SettingsSearchTextBox searchTextBox = null!;

            AddStep("set text", () => (searchTextBox = settings.SectionsContainer.ChildrenOfType<SettingsSearchTextBox>().First()).Current.Value = "some text");
            AddAssert("no text selected", () => searchTextBox.SelectedText == string.Empty);
            AddRepeatStep("toggle visibility", () => settings.ToggleVisibility(), 2);
            AddAssert("search text selected", () => searchTextBox.SelectedText == searchTextBox.Current.Value);
        }

        [Test]
        public void TestServerSettingsSectionVisible()
        {
            AddUntilStep("wait for settings to load", () => settings.SectionsContainer.ChildrenOfType<IFilterable>().Any());
            AddAssert("server settings visible", () => settings.SectionsContainer.ChildrenOfType<ServerSettings>().Any());

            foreach (string service in new[] { "Spectator", "Multiplayer", "Metadata" })
            {
                AddAssert($"{service.ToLowerInvariant()} socket server URL field visible", () => settings.SectionsContainer
                                                                                                   .ChildrenOfType<ServerSettings>()
                                                                                                   .SelectMany(s => s.ChildrenOfType<FormTextBox>())
                                                                                                   .Any(t => t.Caption.ToString().StartsWith($"{service} socket server URL", StringComparison.Ordinal)));
            }
        }

        [Test]
        public void TestServerSettingsRequireOfflineStateToEdit()
        {
            ServerSettings serverSettings = null!;
            FormCheckBox useCustomServerToggle = null!;
            FormTextBox serverUrlTextBox = null!;
            SettingsButtonV2 revertToDefaultButton = null!;

            AddUntilStep("wait for server settings", () => (serverSettings = settings.SectionsContainer.ChildrenOfType<ServerSettings>().SingleOrDefault()) != null);
            scrollIntoView(() => serverSettings);
            AddStep("grab custom server controls", () =>
            {
                useCustomServerToggle = serverSettings.ChildrenOfType<FormCheckBox>().First();
                serverUrlTextBox = serverSettings.ChildrenOfType<FormTextBox>().First();
                revertToDefaultButton = serverSettings.ChildrenOfType<SettingsButtonV2>().First();
            });

            // The dummy API starts out online, so go offline first: otherwise the "disabled while online" checks below would
            // pass without the section ever having reacted to a change.
            AddStep("set API offline", () => dummyAPI.SetState(APIState.Offline));
            AddUntilStep("custom server toggle enabled", () => !useCustomServerToggle.Current.Disabled);
            AddAssert("revert button disabled when custom server off", () => !revertToDefaultButton.Enabled.Value);

            AddStep("set API online", () => dummyAPI.SetState(APIState.Online));
            AddUntilStep("custom server toggle disabled", () => useCustomServerToggle.Current.Disabled);
            AddAssert("server URL textbox read-only", () => serverUrlTextBox.ReadOnly);
            AddAssert("server URL bindable is not disabled", () => !serverUrlTextBox.Current.Disabled);
            AddAssert("revert button disabled while online", () => !revertToDefaultButton.Enabled.Value);

            AddStep("set API offline again", () => dummyAPI.SetState(APIState.Offline));
            AddUntilStep("custom server toggle enabled again", () => !useCustomServerToggle.Current.Disabled);

            AddStep("enable custom server", () => useCustomServerToggle.Current.Value = true);
            AddUntilStep("server URL textbox editable", () => !serverUrlTextBox.ReadOnly);
            AddAssert("revert button enabled when custom server is on", () => revertToDefaultButton.Enabled.Value);

            AddStep("set API online once more", () => dummyAPI.SetState(APIState.Online));
            AddUntilStep("revert button disabled again while online", () => !revertToDefaultButton.Enabled.Value);
        }

        [Test]
        public void TestServerSettingsStateChangesFromBackgroundThread()
        {
            ServerSettings serverSettings = null!;
            FormCheckBox useCustomServerToggle = null!;

            AddUntilStep("wait for server settings", () => (serverSettings = settings.SectionsContainer.ChildrenOfType<ServerSettings>().SingleOrDefault()) != null);
            scrollIntoView(() => serverSettings);
            AddStep("grab custom server toggle", () => useCustomServerToggle = serverSettings.ChildrenOfType<FormCheckBox>().First());

            AddStep("set API offline on background thread", () => Task.Run(() => dummyAPI.SetState(APIState.Offline)).WaitSafely());
            AddUntilStep("custom server toggle enabled", () => !useCustomServerToggle.Current.Disabled);

            AddStep("set API online on background thread", () => Task.Run(() => dummyAPI.SetState(APIState.Online)).WaitSafely());
            AddUntilStep("custom server toggle disabled", () => useCustomServerToggle.Current.Disabled);

            AddStep("set API offline on background thread again", () => Task.Run(() => dummyAPI.SetState(APIState.Offline)).WaitSafely());
            AddUntilStep("custom server toggle enabled again", () => !useCustomServerToggle.Current.Disabled);
        }

        /// <summary>
        /// Scrolls <paramref name="target"/> into view. Drawables outside the visible area are not updated, so a settings
        /// section further down the list would never react to anything (such as API state changes) until it is shown.
        /// </summary>
        private void scrollIntoView(Func<Drawable> target)
        {
            AddStep("scroll into view", () => settings.SectionsContainer.ScrollTo(target()));
            AddUntilStep("is on screen", () => settings.SectionsContainer.ScreenSpaceDrawQuad.AABBFloat.IntersectsWith(target().ScreenSpaceDrawQuad.AABBFloat));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(dialogOverlay = new DialogOverlay
            {
                Depth = -1
            });

            Dependencies.CacheAs<IDialogOverlay>(dialogOverlay);
        }
    }
}
