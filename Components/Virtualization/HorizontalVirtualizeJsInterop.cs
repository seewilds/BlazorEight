// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorSandboxEight.Virtualization;

internal sealed class HorizontalVirtualizeJsInterop : IAsyncDisposable
{
    private const string JsFunctionsPrefix = "Blazor._internal.Virtualize";

    private readonly IVirtualizeJsCallbacks _owner;

    private readonly IJSRuntime _jsRuntime;

    // me
    private IJSObjectReference? _module; 

    private DotNetObjectReference<HorizontalVirtualizeJsInterop>? _selfReference;

    [DynamicDependency(nameof(OnSpacerBeforeVisible))]
    [DynamicDependency(nameof(OnSpacerAfterVisible))]
    public HorizontalVirtualizeJsInterop(IVirtualizeJsCallbacks owner, IJSRuntime jsRuntime)
    {
        _owner = owner;
        _jsRuntime = jsRuntime;
    }

    public async ValueTask InitializeAsync(ElementReference spacerBefore, ElementReference spacerAfter)
    {
        _selfReference = DotNetObjectReference.Create(this);
        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/HorizontalVirtualize.js");
        await _module.InvokeVoidAsync($"init", _selfReference, spacerBefore, spacerAfter);
    }

    [JSInvokable]
    public void OnSpacerBeforeVisible(float spacerSize, float spacerSeparation, float containerSize)
    {
        _owner.OnBeforeSpacerVisible(spacerSize, spacerSeparation, containerSize);
    }

    [JSInvokable]
    public void OnSpacerAfterVisible(float spacerSize, float spacerSeparation, float containerSize)
    {
        _owner.OnAfterSpacerVisible(spacerSize, spacerSeparation, containerSize);
    }

    public async ValueTask DisposeAsync()
    {
        if (_selfReference != null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync($"dispose", _selfReference);
            }
            catch (JSDisconnectedException)
            {
                // If the browser is gone, we don't need it to clean up any browser-side state
            }
        }
    }
}