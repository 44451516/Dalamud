using System;
using System.Runtime.InteropServices;

using Dalamud.Data;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui;
using Dalamud.Game.Network.Internal;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.IoC.Internal;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using Serilog;

namespace Dalamud.Game.ClientState;

/// <summary>
/// This class represents the state of the game client at the time of access.
/// </summary>
[PluginInterface]
[InterfaceVersion("1.0")]
[ServiceManager.BlockingEarlyLoadedService]
#pragma warning disable SA1015
[ResolveVia<IClientState>]
#pragma warning restore SA1015
public sealed class ClientState : IDisposable, IServiceType, IClientState
{
    private readonly GameLifecycle lifecycle;
    private readonly ClientStateAddressResolver address;
    private readonly Hook<SetupTerritoryTypeDelegate> setupTerritoryTypeHook;

    [ServiceManager.ServiceDependency]
    private readonly Framework framework = Service<Framework>.Get();

    [ServiceManager.ServiceDependency]
    private readonly NetworkHandlers networkHandlers = Service<NetworkHandlers>.Get();

    private bool lastConditionNone = true;
    private bool lastFramePvP = false;

    [ServiceManager.ServiceConstructor]
    private ClientState(SigScanner sigScanner, DalamudStartInfo startInfo, GameLifecycle lifecycle)
    {
        this.lifecycle = lifecycle;
        this.address = new ClientStateAddressResolver();
        this.address.Setup(sigScanner);

        Log.Verbose("===== C L I E N T  S T A T E =====");

        this.ClientLanguage = startInfo.Language;

        Log.Verbose($"SetupTerritoryType address 0x{this.address.SetupTerritoryType.ToInt64():X}");

        this.setupTerritoryTypeHook = Hook<SetupTerritoryTypeDelegate>.FromAddress(this.address.SetupTerritoryType, this.SetupTerritoryTypeDetour);

        this.framework.Update += this.FrameworkOnOnUpdateEvent;

        this.networkHandlers.CfPop += this.NetworkHandlersOnCfPop;
    }

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr SetupTerritoryTypeDelegate(IntPtr manager, ushort terriType);

    /// <inheritdoc/>
    public event EventHandler<ushort> TerritoryChanged;

    /// <inheritdoc/>
    public event EventHandler Login;

    /// <inheritdoc/>
    public event EventHandler Logout;

    /// <inheritdoc/>
    public event Action EnterPvP;

    /// <inheritdoc/>
    public event Action LeavePvP;

    /// <inheritdoc/>
    public event EventHandler<Lumina.Excel.GeneratedSheets.ContentFinderCondition> CfPop;

    /// <inheritdoc/>
    public ClientLanguage ClientLanguage { get; }

    /// <inheritdoc/>
    public ushort TerritoryType { get; private set; }

    /// <inheritdoc/>
    public PlayerCharacter? LocalPlayer => Service<ObjectTable>.GetNullable()?[0] as PlayerCharacter;

    /// <inheritdoc/>
    public ulong LocalContentId => (ulong)Marshal.ReadInt64(this.address.LocalContentId);

    /// <inheritdoc/>
    public bool IsLoggedIn { get; private set; }

    /// <inheritdoc/>
    public bool IsPvP { get; private set; }

    /// <inheritdoc/>
    public bool IsPvPExcludingDen { get; private set; }

    /// <summary>
    /// Gets client state address resolver.
    /// </summary>
    internal ClientStateAddressResolver AddressResolver => this.address;

    /// <summary>
    /// Dispose of managed and unmanaged resources.
    /// </summary>
    void IDisposable.Dispose()
    {
        this.setupTerritoryTypeHook.Dispose();
        this.framework.Update -= this.FrameworkOnOnUpdateEvent;
        this.networkHandlers.CfPop -= this.NetworkHandlersOnCfPop;
    }

    [ServiceManager.CallWhenServicesReady]
    private void ContinueConstruction()
    {
        this.setupTerritoryTypeHook.Enable();
    }

    private IntPtr SetupTerritoryTypeDetour(IntPtr manager, ushort terriType)
    {
        this.TerritoryType = terriType;
        this.TerritoryChanged?.InvokeSafely(this, terriType);

        Log.Debug("TerritoryType changed: {0}", terriType);

        return this.setupTerritoryTypeHook.Original(manager, terriType);
    }

    private void NetworkHandlersOnCfPop(object sender, Lumina.Excel.GeneratedSheets.ContentFinderCondition e)
    {
        this.CfPop?.InvokeSafely(this, e);
    }

    private void FrameworkOnOnUpdateEvent(Framework framework1)
    {
        var condition = Service<Conditions.Condition>.GetNullable();
        var gameGui = Service<GameGui>.GetNullable();
        var data = Service<DataManager>.GetNullable();

        if (condition == null || gameGui == null || data == null)
            return;

        if (condition.Any() && this.lastConditionNone == true && this.LocalPlayer != null)
        {
            Log.Debug("Is login");
            this.lastConditionNone = false;
            this.IsLoggedIn = true;
            this.Login?.InvokeSafely(this, null);
            gameGui.ResetUiHideState();

            this.lifecycle.ResetLogout();
        }

        if (!condition.Any() && this.lastConditionNone == false)
        {
            Log.Debug("Is logout");
            this.lastConditionNone = true;
            this.IsLoggedIn = false;
            this.Logout?.InvokeSafely(this, null);
            gameGui.ResetUiHideState();

            this.lifecycle.SetLogout();
        }

        this.IsPvP = GameMain.IsInPvPArea();
        this.IsPvPExcludingDen = this.IsPvP && this.TerritoryType != 250;

        if (this.IsPvP != this.lastFramePvP)
        {
            this.lastFramePvP = this.IsPvP;

            if (this.IsPvP)
            {
                this.EnterPvP?.InvokeSafely();
            }
            else
            {
                this.LeavePvP?.InvokeSafely();
            }
        }
    }
}
