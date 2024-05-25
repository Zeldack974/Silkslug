using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silkslug.ColosseumRubicon
{
    public static class FakeAchievementManagerHooks
    {
        public static void Register()
        {
            On.MainLoopProcess.GrafUpdate += MainLoopProcess_GrafUpdate;
            On.MainLoopProcess.ShutDownProcess += MainLoopProcess_ShutDownProcess;
        }

        private static void MainLoopProcess_ShutDownProcess(On.MainLoopProcess.orig_ShutDownProcess orig, MainLoopProcess self)
        {
            //FakeAchievementManager.instance?.ShutDownProcess();
            orig(self);
        }

        private static void MainLoopProcess_GrafUpdate(On.MainLoopProcess.orig_GrafUpdate orig, MainLoopProcess self, float timeStacker)
        {
            orig(self, timeStacker);
            FakeAchievementManager.instance?.GrafUpdate(timeStacker);
        }
    }
}
