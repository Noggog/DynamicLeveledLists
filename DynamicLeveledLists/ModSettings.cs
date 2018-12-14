using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicLeveledLists
{
    public partial class ModSettings
    {
        public static readonly IModSettingsGetter Defaults = new ModSettings()
        {
            Enabled = true,
            LowTierReductionLine = 2,
            LowTierCutLine = 10,
            HighTierReductionLine = 2,
            HighTierCutLine = 8,
            EpicSpawnsEnabled = true,
            EpicTierSoftCutLine = 8,
            EpicTierCutLine = 15,
            EpicTierPercentChance = 3,
            ForceTrueLevels = true,
            ReviveDeadLLists = true,
            Count = new CountSettings()
            {
                Enabled = true,
                MaxToSpawn = 3,
                BasePercent = 20,
                FinalPercent = 25
            },
            Debug = new DebugSettings()
            {
                InGameSpawningConsoleLogs = false,
                SpawningTracker = false,
            },
            Performance = new SpawningPerformance()
            {
                Delay = false,
                Cleanup = false,
                CleanupBatch = 200,
                Confirm = false,
            },
        };
    }
}
