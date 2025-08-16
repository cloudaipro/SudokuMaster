using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GNPXcore;

namespace GNPXcore
{

    public partial class NuPz_Win
    {
        public long id_obj { get; set; } = DateTime.Now.Ticks;
        // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==
        public GNPX_App GNPX_000;

        public SDK_Ctrl pSDKCntrl => GNPX_000.SDKCntrl;
        public GNPX_AnalyzerMan pAnMan => pGNPX_Eng.AnMan;
        public GNPZ_Engin pGNPX_Eng => GNPX_000.pGNPX_Eng;
        public UPuzzleMan pGPMan => pGNPX_Eng.GPMan;
        public UPuzzle pGP => pGPMan.pGP;      // current board
        public int stageNo => (pGPMan is null) ? 0 : pGPMan.stageNo;

        // ----- timer -----
        private Stopwatch AnalyzerLap;
        private string AnalyzerLapElaped
        {
            get
            {
                TimeSpan ts = AnalyzerLap.Elapsed;
                string st = "";
                if (ts.TotalSeconds > 1.0) st += ts.TotalSeconds.ToString("0.0") + " sec";
                else st += ts.TotalMilliseconds.ToString("0.0") + " msec";
                return st;
            }
        }

        // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*==
        public static NuPz_Win Instance;  //alex
        public NuPz_Win()
        {
            try
            {
                Instance = this; //alex;
                GNPX_000 = new GNPX_App(this);
                AnalyzerLap = new Stopwatch();
                UnityEngine.Debug.Log("call GetMethodListFromFile start...");
                GNPX_000.GetMethodListFromFile();
                UnityEngine.Debug.Log("call GetMethodListFromFile end.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\r" + e.StackTrace);
            }
        }
    }
}