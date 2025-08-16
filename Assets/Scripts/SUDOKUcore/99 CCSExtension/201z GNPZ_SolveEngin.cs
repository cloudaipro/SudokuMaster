using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Text;
using static System.Diagnostics.Debug;
using static System.Math;

namespace GNPXcore
{
    public partial class GNPZ_Engin
    {                        // This class-object is unique in the system.
        // 1-step solver
        public (bool, string) my_sudoku_Solver_SingleStage(bool SolInfoB)
        {
            if (__ChkPrint__) WriteLine($"\n### stageNo:{stageNo}");

            bool mltAnsSearcB = SDK_Ctrl.MltAnsSearch;
            bool AnalysisResult = false;
            int mCC = 0, notFixedCells = 0, freeDigits = 0;

            // --- Initialize ---           
            pGP.Sol_ResultLong = "";
            var (lvlLow, lvlHgh) = Set_AcceptableLevel();
            AnMan.Initialize_Solvers();
            if (GPMan.method_maxDif is null) GPMan.method_maxDif = MethodLst_Run.First();

            do
            {
                try
                {
                    if (pBDL.All(p => (p.FreeB == 0))) break; // All cells confirmed.
                    #region  Verify the solution
                    if (!AnMan.Verify_SUDOKU_Roule())
                    {
                        if (SolInfoB) pGP.Sol_ResultLong = "no solution";
                        return (false, "no solution");
                    }
                    #endregion  Verify the solution

                    DateTime MltAnsTimer = DateTime.Now;

                    //===================================================================================
                    pGP.SolCode = -1;
                    bool L1SolFound = false;
                    foreach (var P in MethodLst_Run)
                    {                                  // Sequentially execute analysis algorithms
                        #region Execution/interruption of analysis method by difficulty 
                        if (L1SolFound && P.DifLevel >= 2) goto LBreak_Analyzing;       //Stop if there is a solution with a difficulty level of 2 or less.

                        int lvl = P.DifLevel;
                        int lvlAbs = Abs(lvl);
                        if (lvlAbs > lvlHgh) continue;                                // Exclude methods with difficulty above the limit
                        if (!mltAnsSearcB && lvl < 0) continue;  // The negative level algorithm is used only with multiple soluving.
                        #endregion Execution/interruption of analysis method by difficulty 

                        #region Algorithm execution
                        try
                        {
                            if (__ChkPrint__) WriteLine($"---> stageNo:{stageNo}  DifLevel:{P.DifLevel}  method: {(mCC++)} - {P.MethodName}");
                            // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*==*
                            GNPZ_Engin.AnalyzingMethodName = P.MethodName;// to display the method in action
                            AnalysisResult = P.MyMethod();            // <--- Execute
                            // *==*==*==*==*==*==*==*==*==*==*==*==*==*==*

                            if (AnalysisResult)
                            { // Solvrd
                                if (__ChkPrint__) WriteLine($"========================> solved {P.MethodName}");
                                if (P.DifLevel >= GPMan.method_maxDif.DifLevel) GPMan.method_maxDif = P;

                                // --- analysis is successful!  save the method and difficulty.
                                if (P.DifLevel <= 2) L1SolFound = true;
                                P.UsedCC++;            // Counter for the number of times the algorithm has been applied
                                pGP.pMethod = P;
                                pGP.DifLevel = Max(pGP.DifLevel, P.DifLevel); // Set the maximum level of the algorithm to the problem level

                                if (!mltAnsSearcB) goto LBreak_Analyzing;  // Abort if single search
                                // -------------------------------------------

                                if ((string)GNPX_App.GMthdOption["abortResult"] != "")
                                {
                                    AnalyzingMethodName = (string)GNPX_App.GMthdOption["abortResult"];
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            WriteLine(e.Message + "\r" + e.StackTrace);
                            using (var fpW = new StreamWriter("Exception_in_Engin.txt", true, Encoding.UTF8))
                            { // message for debug 
                                fpW.WriteLine($"---{DateTime.Now} {e.Message} \r{e.StackTrace}");
                            }
                            break;
                        }
                        #endregion  Algorithm execution
                    }
                    //----------------------------------------------------------------------------

                    if (mltAnsSearcB)
                    {
                        if (child_GPs.Count > 0)
                        {
                            pGP = child_GPs.First();
                            SDK_Ctrl.UGPMan.pGP = pGP;
                            AnalysisResult = true;
                            goto LBreak_Analyzing;
                        }
                    }

                    if (__ChkPrint__) WriteLine("========================> can not solve");
                    if (SolInfoB) pGP.Sol_ResultLong = "no solution";
                    return (false, "no solution");

                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    WriteLine(e.Message + "\r" + e.StackTrace);
                    using (var fpW = new StreamWriter("ExceptionXXX_2.txt", true, Encoding.UTF8))
                    {
                        fpW.WriteLine($"---{DateTime.Now} {e.Message} \r{e.StackTrace}");
                    }
                    break;
                }
                finally
                {
                    SDK_Ctrl.solLevel_notFixedCells = notFixedCells = pBDL.Count(p => (p.FreeB != 0));
                    SDK_Ctrl.solLevel_freeDigits = freeDigits = pBDL.Aggregate(0, (t, p) => t + p.FreeBC);
                    // WriteLine( $"### solLevel_notFixedCells:{notFixedCells}  solLevel_freeDigits:{freeDigits}" ); // for debug
                }

            } while (notFixedCells > 0);

            if (notFixedCells <= 0) { AnalysisResult = true; }  //solved

        LBreak_Analyzing:  //found
            GPMan.selectedIX = (child_GPs != null && child_GPs.Count > 0) ? 0 : -1;
            return (AnalysisResult, "");  // "": "solved"  

            // --------------------------------------- inner functions ---------------------------------------
            (int, int) Set_AcceptableLevel()
            {
                string AnalyzerMode = pGNP00.AnalyzerMode;
                int _lvlLow = 1;
                int _lvlHgh = 5;

                if (AnalyzerMode == "CreatePuzzle")
                {
                    _lvlLow = SDK_Ctrl.lvlLow;
                    _lvlHgh = SDK_Ctrl.lvlHgh;
                }
                else if (AnalyzerMode == "Solve" || AnalyzerMode == "SolveUp")
                {     // ... ?
                    _lvlLow = 1;
                    _lvlHgh = 20;
                }
                else if (AnalyzerMode == "MultiSolve")
                {
                    _lvlLow = 1;
                    _lvlHgh = (int)GNPX_App.GMthdOption["MSlvrMaxAlgorithm"];
                }
                return (_lvlLow, _lvlHgh);
            }

        }
    }
}