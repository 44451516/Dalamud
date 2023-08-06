﻿using System.Diagnostics;

using Dalamud.Hooking;
using Dalamud.Utility.Signatures;

namespace Dalamud.Plugin.Services;

/// <summary>
/// Service responsible for the creation of hooks.
/// </summary>
public interface IHookProvider
{
    /// <summary>
    /// Available hooking backends.
    /// </summary>
    public enum HookBackend
    {
        /// <summary>
        /// Choose the best backend automatically.
        /// </summary>
        Automatic,
        
        /// <summary>
        /// Use Reloaded hooks.
        /// </summary>
        Reloaded,
        
        /// <summary>
        /// Use MinHook.
        /// You should never have to use this without talking to us first.
        /// </summary>
        MinHook,
    }
    
    /// <summary>
    /// Initialize <see cref="Hook{T}"/> members decorated with the <see cref="SignatureAttribute"/>.
    /// Errors for fallible signatures will be logged.
    /// </summary>
    /// <param name="self">The object to initialise.</param>
    public void InitializeFromAttributes(object self);
    
    /// <summary>
    /// Creates a hook by rewriting import table address.
    /// </summary>
    /// <param name="address">A memory address to install a hook.</param>
    /// <param name="detour">Callback function. Delegate must have a same original function prototype.</param>
    /// <returns>The hook with the supplied parameters.</returns>
    /// <typeparam name="T">Delegate of detour.</typeparam>
    public Hook<T> FromFunctionPointerVariable<T>(IntPtr address, T detour) where T : Delegate;
    
    /// <summary>
    /// Creates a hook by rewriting import table address.
    /// </summary>
    /// <param name="module">Module to check for. Current process' main module if null.</param>
    /// <param name="moduleName">Name of the DLL, including the extension.</param>
    /// <param name="functionName">Decorated name of the function.</param>
    /// <param name="hintOrOrdinal">Hint or ordinal. 0 to unspecify.</param>
    /// <param name="detour">Callback function. Delegate must have a same original function prototype.</param>
    /// <returns>The hook with the supplied parameters.</returns>
    /// <typeparam name="T">Delegate of detour.</typeparam>
    public Hook<T> FromImport<T>(ProcessModule? module, string moduleName, string functionName, uint hintOrOrdinal, T detour) where T : Delegate;

    /// <summary>
    /// Creates a hook. Hooking address is inferred by calling to GetProcAddress() function.
    /// The hook is not activated until Enable() method is called.
    /// Please do not use MinHook unless you have thoroughly troubleshot why Reloaded does not work.
    /// </summary>
    /// <param name="moduleName">A name of the module currently loaded in the memory. (e.g. ws2_32.dll).</param>
    /// <param name="exportName">A name of the exported function name (e.g. send).</param>
    /// <param name="detour">Callback function. Delegate must have a same original function prototype.</param>
    /// <param name="backend">Hooking library to use.</param>
    /// <returns>The hook with the supplied parameters.</returns>
    /// <typeparam name="T">Delegate of detour.</typeparam>
    Hook<T> FromSymbol<T>(string moduleName, string exportName, T detour, HookBackend backend = HookBackend.Automatic) where T : Delegate;

    /// <summary>
    /// Creates a hook. Hooking address is inferred by calling to GetProcAddress() function.
    /// The hook is not activated until Enable() method is called.
    /// Please do not use MinHook unless you have thoroughly troubleshot why Reloaded does not work.
    /// </summary>
    /// <param name="procAddress">A memory address to install a hook.</param>
    /// <param name="detour">Callback function. Delegate must have a same original function prototype.</param>
    /// <param name="backend">Hooking library to use.</param>
    /// <returns>The hook with the supplied parameters.</returns>
    /// <typeparam name="T">Delegate of detour.</typeparam>
    Hook<T> FromAddress<T>(IntPtr procAddress, T detour, HookBackend backend = HookBackend.Automatic) where T : Delegate;
    
    /// <summary>
    /// Creates a hook from a signature into the Dalamud target module.
    /// </summary>
    /// <param name="signature">Signature of function to hook.</param>
    /// <param name="detour">Callback function. Delegate must have a same original function prototype.</param>
    /// <param name="backend">Hooking library to use.</param>
    /// <returns>The hook with the supplied parameters.</returns>
    /// <typeparam name="T">Delegate of detour.</typeparam>
    Hook<T> FromSignature<T>(string signature, T detour, HookBackend backend = HookBackend.Automatic) where T : Delegate;
}
