using NLog;
using Torch.Managers.PatchManager;

namespace RPGPlugin.Patches
{
    [PatchShim]
    public static class DrillPatch
    {
        private static Logger log = LogManager.GetLogger("DrillPatch");

        public static void NewDrill()
        {
            // New drill code here
        }

        public static void Patch(PatchContext ctx)
        {
            NewDrill();
        }
        
        
    }
}