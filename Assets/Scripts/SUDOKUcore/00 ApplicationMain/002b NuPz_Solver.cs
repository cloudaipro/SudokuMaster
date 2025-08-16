using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GIDOO_space;
using System.Threading.Tasks;
using System.Threading;

namespace GNPXcore
{
    public partial class NuPz_Win
    {
        #region analysis
        //[Note] task,ProgressChanged,Completed,Canceled threadSafe（Prohibition of control operation）  
        #region analysis[Step] 
        private int OnWork = 0;
        private bool ErrorStopB;
        private int _objectKeyMemo = 0;

        private Task taskSDK;
        private CancellationTokenSource tokSrc;
        
        private void __task_SDKsolver_Completed()
        {
            __DispMode = "Complated";
            taskSDK = null;
            GNPZ_Engin.SolverBusy = false;
            //Dispatcher.RunOnMainThread(() =>
            //{
            //    GameEvents.OnSDKsolverCompletedMethod(Instance.pGNPX_Eng.pGP);
            //});
            
            //alex displayTimer.Start();  //  Conflict-free display start
        }

        public List<_MethodCounter> Get_MethodCounter()
        {  // Aggregation of methods used
            List<_MethodCounter> methodCounters = new List<_MethodCounter>();

            var _MethodCounter = new Dictionary<string, int>();
            var Q = pGNPX_Eng.GPMan;

            if (false)
            {  //---- for debug ... solved.
                bool checkKey = (Q._objectKey == _objectKeyMemo);
                if (checkKey) return methodCounters;                     // Omit if same UPuzzleMan-object.
                _objectKeyMemo = Q._objectKey;
            }

            if (Q.stageNo == 0) { return methodCounters; }

            while (Q != null && Q.pGP != null && Q.stageNo > 0)
            {
                if (Q.pGP.pMethod != null)
                {
                    string keyString = Q.pGP.pMethod.MethodKey;     //"ID"+MethodName
                    if (!_MethodCounter.ContainsKey(keyString)) _MethodCounter[keyString] = 0;
                    _MethodCounter[keyString] += 1;
                }
                Q = Q.GPManPre;
            }

            if (_MethodCounter.Count > 0)
            {
                foreach (var q in _MethodCounter) methodCounters.Add(new _MethodCounter(q.Key, q.Value));

                methodCounters.Sort((a, b) => (a.ID - b.ID));
            }
            return methodCounters;
        }

        public class _MethodCounter
        {
            public int ID;
            public string methodName { get; set; }
            public string difficulty { get; set; }
            public string count { get; set; }
            public int score { get; set; }
            public _MethodCounter(string nm, int cc)
            {
                ID = nm.Substring(0, 7).ToInt();
                methodName = " " + nm.Substring(9);//.PadRight(30);
                difficulty = nm.Substring(7, 2) + "  ";//.PadRight(30);
                count = cc.ToString() + "  ";
            }

            public _MethodCounter((string, int) Q)
            {
                methodName = " " + Q.Item1;//.PadRight(30);
                count = Q.Item2.ToString() + " ";
            }
            public _MethodCounter()
            {
            }
        }

        public void SuDoKuSolver()
        {        // 202303-beta
            try
            {
                if (GNPX_000.AnalyzerMode == "Solve" || GNPX_000.AnalyzerMode == "MultiSolve")
                {

                    if (GNPX_000.pGP.SolCode < 0) GNPX_000.pGP.SolCode = 0;
                    ErrorStopB = !Set_CellsTruth_sub();

                    if ((pGP.BDL).Count(p => p.No == 0) == 0)
                    { //analysis completed
                      //alex _SetScreenProblem();
                        goto AnalyzerEnd;
                    }

                    #region Prepare
                    {
                        OnWork = 1;
                        //alex
                        /*
                        txbStepCC.Text = stageNo.ToString();
                            btnSolve.Content = pRes.msgSuspend;
                            Lbl_onAnalyzing.Content = pRes.Lbl_onAnalyzing;

                            txbStepMCC.Text = txbStepCC.Text;
                            btnMultiSolve.Content = btnSolve.Content;
                            Lbl_onAnalyzingM.Content = Lbl_onAnalyzing.Content;

                            Lbl_onAnalyzing.Foreground = Brushes.Orange;
                            Lbl_onAnalyzingM.Foreground = Brushes.Orange;
                            Lbl_onAnalyzingTS.Content = "";
                            Lbl_onAnalyzingTSM.Content = "";
                            this.Cursor = Cursors.Wait;
                        */

                        if (GNPX_000.SDKCntrl.retNZ == 0) GNPX_000.SDKCntrl.LoopCC = 0;

                        SDK_Ctrl.MltProblem = 1;    //single
                        SDK_Ctrl.lvlLow = 0;
                        SDK_Ctrl.lvlHgh = 999;
                        SDK_Ctrl.NumRandmize = false;
                        //GNPX_000.SDKCntrl.CbxDspNumRandmize=false;

                        GNPX_000.SDKCntrl.GenLStyp = 1;

                        //alex GNPX_App.chbConfirmMultipleCells = (bool)chbConfirmMultipleCells.IsChecked;
                        GNPZ_Engin.SolInfoB = true;
                        AnalyzerLap.Reset();
                    }
                    #endregion

                    //==============================================================
                    {//  Solve the problem (solver_task start)
                        if (!ErrorStopB)
                        {
                            __DispMode = "";
                            AnalyzerLap.Start();
                            //==============================================================
                            tokSrc = new CancellationTokenSource();　//for Cancellation 
                            taskSDK = new Task(() => GNPX_000.SDKCntrl.Analyzer_Real(tokSrc.Token), tokSrc.Token);
                            taskSDK.ContinueWith(t => __task_SDKsolver_Completed()); //procedures used on completion
                            taskSDK.Start();
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
                else
                {
                    try
                    {
                        tokSrc.Cancel();
                        taskSDK.Wait();
                        //btnSolve.Content = pRes.btnSolve;
                    }
                    catch (AggregateException e2)
                    {
                        //WriteLine($"{e2.Message}");
                        __DispMode = "Canceled";
                    }
                }

            AnalyzerEnd:
                return;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"{ex.Message}\r{ex.StackTrace}"); }
        }

        #endregion  analysis[Step]

        #region analysis[All]
        public delegate void SDKsolverAutoCompletedCallBack();
        public SDKsolverAutoCompletedCallBack SolveUpCallBack = null;
        private void task_SDKsolverAuto_Completed()
        {
            __DispMode = "Complated";
            //displayTimer.Start();
            SolveUpCallBack?.Invoke();
        }
        public void SolveUpA() // asynchonize
        {
            if (OnWork == 1) return;
            GNPX_000.AnalyzerMode = "SolveUp";
            pGNPX_Eng.MethodLst_Run_Reset();


            // Full analysis 
            pAnMan.Update_CellsState(pGP.BDL, setAllCandidates: true);  // allFlag:true : set all candidates

            // Complate (All cells are confirmed.)
            if (pGP.BDL.All(p => p.No != 0))
            {
                //_SetScreenProblem();
                goto AnalyzerEnd;
            }

            // No Solution
            if (pGP.BDL.Any(p => (p.No == 0 && p.FreeB == 0)))
            {
                //lblAnalyzerResult.Text = pRes.msgNoSolution;
                goto AnalyzerEnd;
            }

            // Preparation
            {
                OnWork = 2;

                _ResetAnalyzer(true); // Clear Analysis Result
                pGNPX_Eng.AnalyzerCounterReset();

                GNPZ_Engin.SolInfoB = true;
                SDK_Ctrl.lvlLow = 0;
                SDK_Ctrl.lvlHgh = 999;

                //displayTimer.Start();
            }


            // solver_task start
            {
                tokSrc = new CancellationTokenSource();
                CancellationToken ct = tokSrc.Token;
                taskSDK = new Task(() => GNPX_000.SDKCntrl.Analyzer_RealAuto(ct), ct);
                taskSDK.ContinueWith(t => task_SDKsolverAuto_Completed());
                AnalyzerLap.Reset();
                taskSDK.Start();
            }

            AnalyzerLap.Start();
            __DispMode = "";

        AnalyzerEnd:
            //displayTimer.Start();
            return;

        }

        public void SolveUp()
        {
            if (OnWork == 1) return;
            GNPX_000.AnalyzerMode = "SolveUp";
            pGNPX_Eng.MethodLst_Run_Reset();


            // Full analysis 
            pAnMan.Update_CellsState(pGP.BDL, setAllCandidates: true);  // allFlag:true : set all candidates

            // Complate (All cells are confirmed.)
            if (pGP.BDL.All(p => p.No != 0))
            {
                //_SetScreenProblem();
                goto AnalyzerEnd;
            }

            // No Solution
            if (pGP.BDL.Any(p => (p.No == 0 && p.FreeB == 0)))
            {
                //lblAnalyzerResult.Text = pRes.msgNoSolution;
                goto AnalyzerEnd;
            }

            // Preparation
            {
                OnWork = 2;

                _ResetAnalyzer(true); // Clear Analysis Result
                pGNPX_Eng.AnalyzerCounterReset();

                GNPZ_Engin.SolInfoB = true;
                SDK_Ctrl.lvlLow = 0;
                SDK_Ctrl.lvlHgh = 999;

                //displayTimer.Start();
            }

            tokSrc = new CancellationTokenSource();
            CancellationToken ct = tokSrc.Token;
            GNPX_000.SDKCntrl.Analyzer_RealAuto(ct);
            task_SDKsolverAuto_Completed();
            __DispMode = "Complated";

        AnalyzerEnd:
            //displayTimer.Start();
            return;

        }

        private void _ResetAnalyzer(bool AllF = true)
        {
            if (OnWork > 0) return;

            pGNPX_Eng.Clear_0();

            pAnMan.ResetAnalysisResult(AllF);
            pGNPX_Eng.AnalyzerCounterReset();
            pGNPX_Eng.GPMan.pGP.pMethod = null;
#if RegularVersion
            GeneralLogicGen.GLtrialCC=0;
#endif
            //displayTimer.Stop();
            //_SetScreenProblem();
        }
        #endregion

        #region MultiAnalysis        
        static public int[,] Sol99sta = new int[9, 9];
        #endregion

        #region analysis[Method aggregation]
        private int AnalyzerCCMemo=0;
        private int AnalyzerMMemo=0;

        private bool Set_CellsTruth_sub() {   //Cell true/false setting processing     //20230220　確認が必要
            if (pGP.SolCode < 0) return false;
            var (codeX, eNChk) = pAnMan.Execute_Fix_Eliminate(pGP.BDL);
            // codeX = 0 : Complete. Go to next stage.
            //         1 : Solved. 
            //        -1 : Error. Conditions are broken.

            if (codeX == -1 && pGP.SolCode == -9119) {
                string st = "";
                for (int h = 0; h < 27; h++) {
                    if (eNChk[h] != 0x1FF) {
                        st += "Candidate #" + (eNChk[h] ^ 0x1ff).ToBitStringNZ(9) + " disappeared in " + _ToHouseName(h) + "\r";
                        pAnMan.SetBG_OnError(h);
                    }
                }

                //alex lblAnalyzerResult.Text=st;
                GNPX_000.pGP.SolCode = pGP.SolCode;
                return false;
            }


            if (pGP.SolCode == -999) {
                //alex lblAnalyzerResult.Text = "Method control error";
                GNPX_000.pGP.SolCode = -1;
            }

            var (_, nP, nZ, nM) = __Set_CellsPZMCount();
            if (nZ == 0) { pAnMan.SolCode = 0; return true; }
            if (nM != AnalyzerMMemo) {
                AnalyzerCCMemo = stageNo;
                AnalyzerMMemo = nM;
            }

            if (nZ == 0) { //alex && (bool)chbSetDifficulty.IsChecked){
                string prbMessage;
                int DifLevel = pGNPX_Eng.Get_DifficultyLevel( out prbMessage );
                pGP.DifLevel = DifLevel;
                //alex nUDDifficultyLevel.Text = DifLevel.ToString();
                //alex lblCurrentnDifficultyLevel.Content = $"Difficulty: {GNPX_000.pGP.pMethod.DifLevel}"; //CurrentLevel
                //alex if (lblAnalyzerResult.Text!="") lblCurrentnDifficultyLevel.Visibility=Visibility.Visible;
                //alex else lblCurrentnDifficultyLevel.Visibility=Visibility.Hidden;
            }
            return true;

             string _ToHouseName( int h ){
                string st="";
                switch(h/9){
                    case 0: st="row";    break;
                    case 1: st="Column"; break;
                    case 2: st="block";  break;
                }
                st += ((h%9)+1).ToString();
                return st;
             }
        }
        
        #endregion

        #endregion analysis
    }


}