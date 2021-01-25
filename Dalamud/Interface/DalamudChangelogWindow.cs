using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin;
using ImGuiNET;
using Lumina.Data.Parsing.Layer;

namespace Dalamud.Interface {
    class DalamudChangelogWindow : IDisposable {
        private readonly Dalamud dalamud;
        private string assemblyVersion = Util.AssemblyVersion;

        private const bool WarrantsChangelog = false;
        private const string ChangeLog =
            @"* Fixed various bugs with plugins not installing correctly or causing crashes in certain situations
* All installed plugins, regardless of their status in the repository, are now shown
* Plugins are now grouped by installation state in the installer

If you have previously encoutered issues with plugins since 5.4, they should be resolved now for most of them.

As this is a major patch and we have made several backend changes, please keep in mind that it may take a little bit for all of your favorite plugins to be available again.";

        public DalamudChangelogWindow(Dalamud dalamud) {
            this.dalamud = dalamud;
        }

        public bool Draw() {
            var doDraw = true;

            if (!WarrantsChangelog)
                return false;

            ImGui.PushID("DalamudChangelogWindow");
            ImGui.Begin("What's new in XIVLauncher?", ref doDraw, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize);
            
            ImGui.Text($"The in-game addon has been updated to version D{this.assemblyVersion}.");

            ImGui.Dummy(new Vector2(10, 10) * ImGui.GetIO().FontGlobalScale);

            ImGui.Text("The following changes were introduced:");
            ImGui.Text(ChangeLog);

            ImGui.Dummy(new Vector2(10, 10) * ImGui.GetIO().FontGlobalScale);

            ImGui.Text("Thank you for using our tools!");

            ImGui.Dummy(new Vector2(10, 10) * ImGui.GetIO().FontGlobalScale);

            ImGui.PushFont(InterfaceManager.IconFont);

            if (ImGui.Button(FontAwesomeIcon.Download.ToIconString())) 
                this.dalamud.DalamudUi.OpenPluginInstaller();

            if (ImGui.IsItemHovered()) {
                ImGui.PopFont();
                ImGui.SetTooltip("Open Plugin Installer");
                ImGui.PushFont(InterfaceManager.IconFont);
            }
            
            ImGui.SameLine();

            if (ImGui.Button(FontAwesomeIcon.LaughBeam.ToIconString())) 
                Process.Start("https://discord.gg/3NMcUV5");

            if (ImGui.IsItemHovered()) {
                ImGui.PopFont();
                ImGui.SetTooltip("Join our Discord server");
                ImGui.PushFont(InterfaceManager.IconFont);
            }

            ImGui.SameLine();

            if (ImGui.Button(FontAwesomeIcon.Globe.ToIconString()))
                Process.Start("https://github.com/goatcorp/FFXIVQuickLauncher");

            if (ImGui.IsItemHovered()) {
                ImGui.PopFont();
                ImGui.SetTooltip("See our GitHub repository");
                ImGui.PushFont(InterfaceManager.IconFont);
            }
                

            ImGui.PopFont();

            ImGui.SameLine();
            ImGui.Dummy(new Vector2(20, 0) * ImGui.GetIO().FontGlobalScale);
            ImGui.SameLine();

            if (ImGui.Button("Close")) {
                doDraw = false;
            }

            ImGui.End();
            ImGui.PopID();

            return doDraw;
        }

        public void Dispose() {

        }
    }
}
