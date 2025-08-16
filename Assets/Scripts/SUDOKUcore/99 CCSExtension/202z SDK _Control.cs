using System;
using System.Linq;
using System.Collections.Generic;
using static System.Diagnostics.Debug;
using static System.Math;
using System.Threading;
using System.Threading.Tasks;

namespace GNPXcore
{

    // ### This class will change in the future.  ............
    //     Organize items into the following classes. ---> GNPX_AnalyzerMan, SDK_Ctrl, GNPZ_Engin, ...

    public partial class SDK_Ctrl
    {               
        public void MyAnalyzer_Real()
        {      //Analysis[step]
            try
            {
                retNZ = -1; LoopCC++; TLoopCC++;
                if (pGNPX_Eng.Set_Methods_for_Solving(false) < 0) return;      // Run every analysis
                pGNPX_Eng.my_sudoku_Solver_SingleStage(true);
                //  SDKEventArgs se = new SDKEventArgs(ProgressPer:retNZ);
                //  Send_Progress(this,se);
            }
            catch (TaskCanceledException)
            {
                WriteLine("...Canceled by user.");
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message + "\r" + ex.StackTrace);
            }
        }
    }
}
