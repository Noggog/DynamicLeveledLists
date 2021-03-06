﻿using Mutagen.Bethesda;
using Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Oblivion.Internals;
using Noggog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;

namespace DynamicLeveledLists
{
    class Program
    {
        const string Prefix = "DLL";
        static Dictionary<FormKey, IMajorRecordCommon> sourceRecords;
        static Dictionary<StringCaseAgnostic, IMajorRecordCommon> sourceEdidRecords = new Dictionary<StringCaseAgnostic, IMajorRecordCommon>();
        static OblivionMod dynamicLeveledLists;
        static ModKey sourceModKey = new ModKey("DLLSource", master: false);
        static ModSettings settings = new ModSettings();
        static ModSettings defaults = new ModSettings()
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
                MaxToSpawn = 3,
                BasePercent = 0.2,
                FinalPercent = 0.25,
            },
            Debug = new DebugSettings()
            {
                InGameSpawningConsoleLogs = false,
                SpawningTracker = false
            },
            Performance = new SpawningPerformance()
            {
                Delay = true,
                Cleanup = false,
                Confirm = true,
                CleanupBatch = 200,
            }
        };

        static async Task Main(string[] args)
        {
            var settingsFilePath = new FilePath($"{args[0]}/MutagenPatcherSettings/DynamicLeveledLists.xml");
            // Eventually refactor to use XML copy in call
            // /w singleton settings object as its own defaults
            if (settingsFilePath.Exists)
            {
                settings = ModSettings.Create_Xml(settingsFilePath.Path);
            }
            else
            {
                settings.CopyFieldsFrom(defaults);
            }
            await OblivionPipeline.TypicalPatch(
                mainArgs: args,
                outputMod: ModKey.Factory("DynamicLeveledLists.esp"),
                importMask: new GroupMask(false)
                {
                    Creatures = true,
                    NPCs = true,
                    LeveledCreatures = true,
                },
                processor: CreateMod);
        }

        static async Task<OblivionMod> CreateMod(ModKey modKey, ModList<OblivionMod> modList)
        {
            var flattenedMod = modList.Flatten();
            dynamicLeveledLists = new OblivionMod(modKey);
            dynamicLeveledLists.TES4.Author = "Noggog";

            CopyOverSource(modList);
            ModifyLLists(flattenedMod);
            SetGlobalSettings();

            return dynamicLeveledLists;
        }

        static OblivionMod GetSourceMod(ModList<OblivionMod> modList)
        {
            if (!File.Exists(Properties.Settings.Default.SourceFile))
            {
                throw new ArgumentException($"DLL source mod could not be found at location: {Properties.Settings.Default.SourceFile}");
            }
            var mod = OblivionMod.Create_Binary(
                Properties.Settings.Default.SourceFile,
                sourceModKey);
            mod.Link(modList);
            return mod;
        }

        static void CopyOverSource(ModList<OblivionMod> modList)
        {
            var source = GetSourceMod(modList);
            sourceRecords = dynamicLeveledLists.CopyInDuplicate(source);
            sourceModKey = source.ModKey;
            sourceEdidRecords.Set(sourceRecords.Values.Select(m => new KeyValuePair<StringCaseAgnostic, IMajorRecordCommon>(m.EditorID, m)));
        }

        static void ModifyLLists(OblivionMod flattenedModList)
        {
            var dummyScript = (Script)sourceEdidRecords["DLLDummyScript"];

            Creature GetDummyRat(FormKey form)
            {
                return new Creature(form)
                {
                    Model = new Model()
                    {
                        BoundRadius = 97.858261f,
                        File = @"Creatures\Rat\Skeleton.NIF"
                    },
                    Flags =
                        Creature.CreatureFlag.Swims |
                        Creature.CreatureFlag.Walks |
                        Creature.CreatureFlag.PCLevelOffset |
                        Creature.CreatureFlag.NoLowLevelProcessing,
                    Fatigue = 60,
                    Script = dummyScript,
                    Confidence = 70,
                    EnergyLevel = 70,
                    Teaches = Skill.Armorer,
                    CreatureType = Creature.CreatureTypeEnum.Creature,
                    CombatSkill = 20,
                    MagicSkill = 50,
                    StealthSkill = 20,
                    SoulLevel = SoulLevel.Petty,
                    Health = 1000,
                    AttackDamage = 2,
                    Strength = 20,
                    Intelligence = 15,
                    Willpower = 1,
                    Agility = 20,
                    Speed = 9,
                    Endurance = 20,
                    Personality = 10,
                    Luck = 20,
                    AttackReach = 96,
                    TurningSpeed = 0,
                    BaseScale = 1,
                    FootWeight = 3,
                };
            }

            int ratCount = 0;
            foreach (var llist in flattenedModList.LeveledCreatures)
            {
                // Modify existing entries to never spawn
                foreach (var entry in llist.Entries)
                {
                    entry.Level += 10000;
                }

                // Create a custom dummy, with name field "pointing" to LList
                var ratNPC = GetDummyRat(dynamicLeveledLists.GetNextFormKey());
                ratNPC.EditorID = $"DLLDummy{ratCount++}";
                ratNPC.Name = $"{llist.FormKey.ToString()}";
                dynamicLeveledLists.Creatures.Items.Set(ratNPC);

                // Add dummy to LList
                llist.Entries.Add(
                    new LeveledEntry<NPCSpawn>()
                    {
                        Level = 1,
                        Count = 1,
                        Reference = ratNPC
                    });

                dynamicLeveledLists.LeveledCreatures.Items.Set(llist);
            }
        }

        static void SetGlobalSettings()
        {
            // Spawning
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLlowTierReducLine"]).Data = settings.LowTierReductionLine;
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLlowTierCutLine"]).Data = settings.LowTierCutLine;
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLHighTierReducLine"]).Data = settings.HighTierReductionLine;
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLHighTierCutLine"]).Data = settings.HighTierCutLine;
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLEpicButton"]).Data = settings.EpicSpawnsEnabled ? ((short)1) : ((short)0);
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLEpicTierCutLine"]).Data = settings.EpicTierCutLine;
            ((GlobalFloat)dynamicLeveledLists.Globals.ByEditorID["DLLEpicTierDropPercent"]).Data = (float)settings.EpicTierPercentChance;
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLEpicTierSoftCutLine"]).Data = settings.EpicTierSoftCutLine;
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLForceLevel"]).Data = settings.ForceTrueLevels ? ((short)1) : ((short)0);
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLReviveEmptyLLists"]).Data = settings.ReviveDeadLLists ? ((short)1) : ((short)0);

            // Count Spawning
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLIncreasedSpawnsButton"]).Data = settings.Count.Enabled ? ((short)1) : ((short)0);
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLSpawnIncreaseMax"]).Data = settings.Count.MaxToSpawn;
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLSpawnIncreaseBasePercent"]).Data = (short)(settings.Count.BasePercent * 100);
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLSpawnIncreaseFinalPercent"]).Data = (short)(settings.Count.FinalPercent * 100);

            // Performance
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLdelaySpawning"]).Data = settings.Performance.Delay ? ((short)1) : ((short)0);
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLconfirmSpawning"]).Data = settings.Performance.Confirm ? ((short)1) : ((short)0);
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLdummyCleanupOption"]).Data = settings.Performance.Cleanup ? ((short)1) : ((short)0);
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLdummyCleanupNumber"]).Data = settings.Performance.CleanupBatch;

            // Debug
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLSpawnTracker"]).Data = settings.Debug.SpawningTracker ? ((short)1) : ((short)0);
            ((GlobalShort)dynamicLeveledLists.Globals.ByEditorID["DLLDebug"]).Data = settings.Debug.InGameSpawningConsoleLogs ? ((short)1) : ((short)0);
        }
    }
}
