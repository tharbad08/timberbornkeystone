# Native terrain excavation compatibility (Miners of Mountains)

## Problem
Keystone decorations supported by excavated ground can block excavation or stay behind without being invalidated. Replacing a blueprint's `MatterBelow: Ground` with `Any` changes the intended placement rules and can cause floating decoration over water.

## Change
- Implement Timberborn's native `IGroundMatterBelowInvalidator` on the existing `KeystoneVariant` persistent component.
- Add `KeystoneVariantSpec` to the two standalone resource blueprints missing it: `Rock_medium_1` and `KeystoneFlourishTest`.
- Keep the blueprint's ground, stacking, occupancy and positioning rules unchanged.
- No new Harmony patch, external adapter, or additional required-mod dependency.

## Optional dependency
MoM is **not** referenced or required. The Timberborn interface exists independently of MoM. This change is intended to work when MoM is not installed, installed but disabled, or enabled. It does not assume that a loaded MoM assembly means MoM is enabled.

## Validation checklist (not yet performed against this exact source)
1. Launch Timberborn 1.1 and Keystone 1.3.3 with MoM absent; create/load colony, verify decorations spawn normally.
2. Repeat with MoM installed but disabled.
3. Enable MoM and excavate under Keystone flowers, flourishes and stones; check excavation completes, ground-supported entities invalidate, and no stuck jobs occur.
4. Save/reload a colony with existing Keystone decoration and repeat excavation.
5. Test support removal next to water, verify no floating decoration and no regression in ordinary ground-only placement.
6. Check `Player.log` for type load errors, and repeat with other terrain modification tools.

**Evidence:** Reviewed source against upstream `cf42782a`; the user-supplied compatibility DLL and Timberborn 1.1 references contain the interface name. Neither an actual build of this commit nor an in-game regression test has been performed here. The supplied older binary is not a compiled artifact of this new branch.
