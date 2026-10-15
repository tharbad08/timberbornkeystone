using System;
using System.Collections.Generic;
using System.Diagnostics;
using Keystone.Mod.Diagnostics;
using Timberborn.TemplateCollectionSystem;
using Timberborn.TemplateInstantiation;

namespace Keystone.Mod.Fauna {

  /// <summary>
  /// Populate the game's own cached fauna templates after all
  /// template collections have loaded, before the first gameplay frame.
  /// KeystoneStartupWarmup invokes this after its regular PostLoad work.
  /// No second cache or early TemplateCollectionService.Load patch.
  /// </summary>
  public sealed class KeystoneFaunaTemplatePrewarmer {

    private static readonly HashSet<string> TargetNames = new(StringComparer.Ordinal) {
        "KeystoneCow", "KeystoneBull", "KeystoneDeer"
    };

    private readonly TemplateCollectionService _templates;
    private readonly TemplateInstantiator _instantiator;
    private bool _completed;

    public KeystoneFaunaTemplatePrewarmer(
        TemplateCollectionService templates,
        TemplateInstantiator instantiator) {
      _templates = templates;
      _instantiator = instantiator;
    }

    /// <summary>Prewarm known fauna templates once per loaded world.</summary>
    public void RunAfterLoad() {
      if (_completed) return;

      try {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var warmed = 0;
        foreach (var blueprint in _templates.AllTemplates) {
          if (blueprint == null || !TargetNames.Contains(blueprint.Name)
              || !seen.Add(blueprint.Name)) {
            continue;
          }

          try {
            var sw = Stopwatch.StartNew();
            _instantiator.CacheInstance(blueprint);
            KeystoneLog.Verbose(
                $"[Keystone] Fauna prewarmed '{blueprint.Name}' in {sw.ElapsedMilliseconds} ms.");
            warmed++;
          } catch (Exception ex) {
            // Don't make an optional allocation optimization prevent
            // gameplay: the vanilla path may retry on first spawn.
            KeystoneLog.Warn(
                $"[Keystone] Fauna prewarm skipped '{blueprint.Name}': "
                + $"{ex.GetType().Name}: {ex.Message}");
          }
        }

        _completed = true;
        KeystoneLog.Verbose(
            $"[Keystone] Fauna template prewarm complete: {warmed}/{TargetNames.Count} "
            + "templates cached using vanilla TemplateInstantiator.");
      } catch (Exception ex) {
        // A partially initialised game collection should never abort
        // the rest of the startup warmup; the original spawn path remains.
        KeystoneLog.Warn(
            $"[Keystone] Fauna template prewarm unavailable: "
            + $"{ex.GetType().Name}: {ex.Message}");
      }
    }
  }
}
