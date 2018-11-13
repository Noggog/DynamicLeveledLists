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
        static Script DummyScript;

        static async Task Main(string[] args)
        {
            await OblivionPipeline.TypicalPatch(
                mainArgs: args,
                outputMod: ModKey.Factory("Test.esp"),
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
            var dynamicLeveledLists = new OblivionMod(modKey);
            dynamicLeveledLists.TES4.Author = "Noggog";

            CopyOverDLLScripts(dynamicLeveledLists);
            AddNPCs(dynamicLeveledLists, flattenedMod);
            AddCreatures(dynamicLeveledLists, flattenedMod);
            AddGlobals(dynamicLeveledLists, flattenedMod);
            ModifyLLists(dynamicLeveledLists, flattenedMod);
            return dynamicLeveledLists;
        }

        static void CopyOverDLLScripts(OblivionMod dynamicLeveledLists)
        {
            if (!File.Exists(Properties.Settings.Default.ScriptSourceFile))
            {
                throw new ArgumentException($"Script source mod could not be found at location: {Properties.Settings.Default.ScriptSourceFile}");
            }
            OblivionMod scriptsSource = OblivionMod.Create_Binary(
                Properties.Settings.Default.ScriptSourceFile,
                new ModKey("ScriptSource", master: false));
            foreach (var script in scriptsSource.Scripts)
            {
                var scriptCopy = new Script(dynamicLeveledLists.GetNextFormKey());
                scriptCopy.CopyFieldsFrom(script);
                dynamicLeveledLists.Scripts.Items.AddOrUpdate(scriptCopy);
                if (scriptCopy.EditorID.Equals("DLLDummyScript"))
                {
                    DummyScript = scriptCopy;
                }
            }

            if (DummyScript == null)
            {
                throw new ArgumentException("Dummy script not found in scripts source mod.");
            }
        }

        static void AddCreatures(OblivionMod dynamicLeveledLists, OblivionMod flattenedModList)
        {
            foreach (var crea in flattenedModList.Creatures)
            {
                // ToDo
                // Modify Calc min / Calc max
                dynamicLeveledLists.Creatures.Items.AddOrUpdate(crea);
            }
        }

        static void AddNPCs(OblivionMod dynamicLeveledLists, OblivionMod flattenedModList)
        {
            foreach (var npc in flattenedModList.NPCs)
            {
                // ToDo
                // Modify Calc min / Calc max
                dynamicLeveledLists.NPCs.Items.AddOrUpdate(npc);
            }
        }

        static void ModifyLLists(OblivionMod dynamicLeveledLists, OblivionMod flattenedModList)
        {
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
                ratNPC.Name = $"DLLdummy";
                dynamicLeveledLists.Creatures.Items.AddOrUpdate(ratNPC);

                // Add dummy to LList
                llist.Entries.Add(
                    new LeveledEntry<NPCSpawn>()
                    {
                        Level = 1,
                        Count = 1,
                        Reference = ratNPC
                    });

                dynamicLeveledLists.LeveledCreatures.Items.AddOrUpdate(llist);
            }
        }
        
        static Creature GetDummyRat(FormKey form)
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
                Script = DummyScript,
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

        static void AddGlobals(OblivionMod mod, OblivionMod flattenedModList)
        {
            mod.Globals.Items.AddOrUpdate(
                new GlobalShort(mod.GetNextFormKey())
                {
                    EditorID = "DLLLimitProcessingCounter",
                    Data = 0
                });
            mod.Globals.Items.AddOrUpdate(
                new GlobalShort(mod.GetNextFormKey())
                {
                    EditorID = "DLLlowTierReducLine",
                    Data = 0
                });
            mod.Globals.Items.AddOrUpdate(
                new GlobalShort(mod.GetNextFormKey())
                {
                    EditorID = "DLLlowTierCutLine",
                    Data = 0
                });
            mod.Globals.Items.AddOrUpdate(
                new GlobalShort(mod.GetNextFormKey())
                {
                    EditorID = "DLLHighTierReducLine",
                    Data = 0
                });
            mod.Globals.Items.AddOrUpdate(
                new GlobalShort(mod.GetNextFormKey())
                {
                    EditorID = "DLLHighTierCutLine",
                    Data = 0
                });
        }
    }
}
