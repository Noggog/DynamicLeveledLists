using Loqui;
using Loqui.Generation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicLeveledLists.Loqui.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            LoquiGenerator gen = new LoquiGenerator()
            {
                RaisePropertyChangedDefault = false,
                NotifyingDefault = NotifyingType.ReactiveUI,
                ObjectCentralizedDefault = true,
                HasBeenSetDefault = false,
            };
            gen.XmlTranslation.ShouldGenerateXSD = true;
            gen.XmlTranslation.ExportWithIGetter = false;

            var proto = gen.AddProtocol(
                new ProtocolGeneration(
                    gen,
                    new ProtocolKey("DynamicLeveledLists"),
                    new DirectoryInfo("../../../DynamicLeveledLists"))
                {
                    DefaultNamespace = "DynamicLeveledLists",
                });
            proto.AddProjectToModify(
                new FileInfo(Path.Combine(proto.GenerationFolder.FullName, "DynamicLeveledLists.csproj")));
            
            gen.Generate().Wait();
        }
    }
}
