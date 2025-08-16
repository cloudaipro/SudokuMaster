using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GIDOO_space;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

namespace GNPXcore
{
    public partial class NuPz_Win
    {
        public void MySolver()
        {        // 202303-beta
            try
            {
                if (GNPX_000.AnalyzerMode == "Solve" || GNPX_000.AnalyzerMode == "MultiSolve")
                {

                    if (GNPX_000.pGP.SolCode < 0) GNPX_000.pGP.SolCode = 0;
                    ErrorStopB = !Set_CellsTruth_sub();

                    if ((pGP.BDL).Count(p => p.No == 0) == 0)
                    { 
                        goto AnalyzerEnd;
                    }

                    #region Prepare
                    {
                        OnWork = 1;                        
                        if (GNPX_000.SDKCntrl.retNZ == 0) GNPX_000.SDKCntrl.LoopCC = 0;

                        SDK_Ctrl.MltProblem = 1;    //single
                        SDK_Ctrl.lvlLow = 0;
                        SDK_Ctrl.lvlHgh = 999;
                        SDK_Ctrl.NumRandmize = false;
                        //GNPX_000.SDKCntrl.CbxDspNumRandmize=false;

                        GNPX_000.SDKCntrl.GenLStyp = 1;

                        //alex GNPX_App.chbConfirmMultipleCells = (bool)chbConfirmMultipleCells.IsChecked;
                        GNPZ_Engin.SolInfoB = true;
                    }
                    #endregion

                    //==============================================================
                    {//  Solve the problem (solver_task start)
                        if (!ErrorStopB)
                        {
                            __DispMode = "";
                            //==============================================================
                            GNPX_000.SDKCntrl.MyAnalyzer_Real();
                            my__task_SDKsolver_Completed();
                        }
                        else
                        {
                            __DispMode = "Complated";
                        }
                    }
                    //--------------------------------------------------------------


                    //if( GNPX_000.AnalyzerMode!="MultiSolve" ) displayTimer.Start(); // <- To avoid unresolved operation trouble.
                    //alex displayTimer.Start();           //  Conflict-free display start
                    //--------------------------------------------------------------         
                }
            AnalyzerEnd:
                return;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"{ex.Message}\r{ex.StackTrace}"); }
        }
        private void my__task_SDKsolver_Completed()
        {
            __DispMode = "Complated";
            GNPZ_Engin.SolverBusy = false;
            GameEvents.OnSDKsolverCompletedMethod(pGNPX_Eng.pGP);
        }
    }


}