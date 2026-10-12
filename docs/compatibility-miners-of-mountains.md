# Optional ground excavation compatibility (Miners of Mountains)

### Problem

Keystone places natural flourishes and decorative rocks above ordinary terrain. Miners of Mountains (MoM) and other excavation tools can reject terrain removal when the supported natural resources have no ground-invalidation marker. Earlier broad workarounds altered `MatterBelow` and resulted in flourishes or stones lingering over water.

### Fix

- `KeystoneVariant` now implements Timberborn's **native** `IGroundMatterBelowInvalidator` marker. Keystone's spawn recipes already attach `KeystoneVariantSpec`, so these entities become eligible for removal as the supporting ground is excavated.
- Add `KeystoneVariantSpec` to two previously-unmarked standalone blueprints: `Rock_medium_1` and `KeystoneFlourishTest`.
- Leave all 96 natural-resource blueprint `MatterBelow: Ground` values intact; preserve occupancy rules and visual placement semantics.
- **MoM is entirely optional.** The patch references only a Timberborn interface, not the MoM mod, and does not call any MoM method or attempt to load an MoM DLL. There is no optional integration to enable, so a conditional MoM presence check is unnecessary and would add a new failure mode.

### Scope

This deliberately does not patch MoM itself, general demolition stall logic, global terrain validation, or Keystone placement on water. `IGroundMatterBelowInvalidator` is a marker with no methods; the game's existing terrain-removal pipeline governs removal.

### Regression checks for the maintainer

1. Launch **Keystone without MoM**, load and start a game; verify no TypeLoadException or missing DLL; ecological decoration creation remains normal.
2. Launch **Keystone with MoM**; excavate below Keystone flourishes and rocks; removal is permitted and decorations disappear.
3. Reload a saved colony containing decor then excavate; the component is reconstructed from the blueprint spec.
4. Verify unsupported/water tiles no longer retain ground-required florishes/rocks, and vanilla placement/overriding still behave normally.
5. Check other terrain/demolition tools for unexpected extra invalidations.

Based on the local `Keystone_1.3.3_MOM_Compat_v7_Clean` build: it contains the marker name and the two additional blueprint specs while retaining the original ground rules. Source-level port only; compile and in-game regression checks remain outstanding.
