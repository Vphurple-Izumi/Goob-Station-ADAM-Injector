// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.AdamInjector;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AdamInjectorComponent : Component
{
    /// <summary>
    /// Whether the injector has been used and is now empty
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsEmpty = false;
}
