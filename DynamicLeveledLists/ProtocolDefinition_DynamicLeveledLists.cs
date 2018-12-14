using Loqui;
using DynamicLeveledLists;

namespace Loqui
{
    public class ProtocolDefinition_DynamicLeveledLists : IProtocolRegistration
    {
        public readonly static ProtocolKey ProtocolKey = new ProtocolKey("DynamicLeveledLists");
        public void Register()
        {
            LoquiRegistration.Register(DynamicLeveledLists.Internals.ModSettings_Registration.Instance);
            LoquiRegistration.Register(DynamicLeveledLists.Internals.DebugSettings_Registration.Instance);
            LoquiRegistration.Register(DynamicLeveledLists.Internals.CountSettings_Registration.Instance);
            LoquiRegistration.Register(DynamicLeveledLists.Internals.SpawningPerformance_Registration.Instance);
        }
    }
}
