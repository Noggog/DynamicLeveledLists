using Mutagen.Bethesda;
using Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Oblivion.Internals;
using Noggog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicLeveledLists
{
    class Program
    {
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

        static async Task<OblivionMod> CreateMod(ModList<OblivionMod> modList)
        {
            var dynamicLeveledLists = new OblivionMod();
            foreach (var mod in modList)
            {
                dynamicLeveledLists.AddRecords(
                    mod.Mod,
                    new GroupMask()
                    {
                        NPCs = true
                    });
            }
            return dynamicLeveledLists;
        }
    }
}
