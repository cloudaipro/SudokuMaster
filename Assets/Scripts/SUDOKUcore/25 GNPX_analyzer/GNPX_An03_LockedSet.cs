using System;
using System.Collections.Generic;
using System.Linq;
using GIDOO_space;
using UnityEngine;

namespace GNPXcore
{
    public class LockedSetGen : AnalyzerBaseV2
    {
        public LockedSetGen(GNPX_AnalyzerMan AnMan) : base(AnMan) { }

        // https://gidoo-code.github.io/Sudoku_Solver_Generator/page33.html

        public bool LockedSet2() => LockedSetSub(2);  //2D
        public bool LockedSet3() => LockedSetSub(3);  //3D
        public bool LockedSet4() => LockedSetSub(4);  //4D
        public bool LockedSet5() => LockedSetSub(5);  //complementary to 4D Hidden
        public bool LockedSet6() => LockedSetSub(6);  //complementary to 3D Hidden
        public bool LockedSet7() => LockedSetSub(7);  //complementary to 2D Hidden

        public bool LockedSet2Hidden() => LockedSetSub(2, HiddenFlag: true); //2D Hidden
        public bool LockedSet3Hidden() => LockedSetSub(3, HiddenFlag: true);  //3D Hidden
        public bool LockedSet4Hidden() => LockedSetSub(4, HiddenFlag: true);  //4D Hidden
        public bool LockedSet5Hidden() => LockedSetSub(5, HiddenFlag: true);  //complementary to 4D 
        public bool LockedSet6Hidden() => LockedSetSub(6, HiddenFlag: true);  //complementary to 3D 
        public bool LockedSet7Hidden() => LockedSetSub(7, HiddenFlag: true);  //complementary to 2D 


        public bool LockedSetSub_original(int sz, bool HiddenFlag = false)
        {
            string resST = "";
            UCell UC;
            for (int h = 0; h < 27; h++)
            {
                List<UCell> BDLstF = pBDL.IEGetCellInHouse(h, 0x1FF).ToList();    //select cells in house
                int ncF = BDLstF.Count;
                if (ncF <= sz) continue;

                Combination cmbG = new Combination(ncF, sz);
                while (cmbG.Successor())
                {
                    BDLstF.ForEach(p => p.Selected = false);        // Initialize the cells in the BDLstF.
                    int selBlk = 0;
                    Array.ForEach(cmbG.Index, p => { (UC = BDLstF[p]).Selected = true; selBlk |= 1 << UC.b; });  //selecte cells by Combination

                    int noBSel = 0, noBNon = 0;
                    BDLstF.ForEach(p => {
                        if (p.Selected) noBSel |= p.FreeB;
                        else noBNon |= p.FreeB;
                    });

                    //   int noBSel = BDLstF.Where(p=> p.Selected).Aggregate(0,(Q,q)=>Q|=q.FreeB);
                    //   int noBNon = BDLstF.Where(p=>!p.Selected).Aggregate(0,(Q,q)=>Q|=q.FreeB);

                    if ((noBSel & noBNon) == 0) continue;                      //any digits that can be excluded?

                    //=============== Naked Locked Set ===============
                    if (!HiddenFlag)
                    {
                        if (noBSel.BitCount() == sz)
                        {                          //Number of selected cell's digits is sz
                            if (h < 18 && selBlk.BitCount() == 1)
                            {
                                // When searching for a row or column and it is one block,
                                // there may be elements within the block that can be excluded.
                                int tfx2 = selBlk.BitToNum() + 18;              //bit expression -> House_No(18-26)
                                List<UCell> PLst = pBDL.IEGetCellInHouse(tfx2, noBSel).ToList();
                                BDLstF.ForEach(P => { if (P.Selected) PLst.Remove(P); });
                                if (PLst.Count > 0) PLst.ForEach(P => { P.CancelB = P.FreeB & noBSel; });
                            }

                            resST = "";
                            foreach (var P in BDLstF)
                            {
                                if (P.Selected)
                                {
                                    P.Set_CellColorBkgColor_noBit(noBSel, AttCr, SolBkCr);
                                    resST += " " + P.rc.ToRCString();
                                }
                                else P.CancelB = P.FreeB & noBSel;
                            }
                            resST = resST.ToString_SameHouseComp() + " #" + noBSel.ToBitStringN(9);
                            _LockedSetResult(sz, resST, HiddenFlag);
                            if (__SimpleAnalyzerB__) return true;
                            if (!pAnMan.SnapSaveGP(pGP)) return true;
                        }
                    }

                    //=============== Hidden Locked Set ===============
                    if (HiddenFlag)
                    {
                        if (noBNon.BitCount() == (ncF - sz))
                        {                    //Number of unselected cell's digits is (ncF-sz)
                            resST = "";
                            foreach (var P in BDLstF.Where(p => p.Selected))
                            {
                                P.CancelB = P.FreeB & noBNon;
                                P.Set_CellColorBkgColor_noBit(noBSel, AttCr, SolBkCr);
                                resST += " " + P.rc.ToRCString();
                            }
                            int nobR = noBSel.DifSet(noBNon);
                            resST = resST.ToString_SameHouseComp() + " #" + nobR.ToBitStringN(9);
                            _LockedSetResult(sz, resST, HiddenFlag);
                            if (__SimpleAnalyzerB__) return true;
                            if (!pAnMan.SnapSaveGP(pGP)) return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool LockedSetSub(int sz, bool HiddenFlag = false)
        {
            string resST = "";
            UCell UC;
            for (int h = 0; h < 27; h++)
            {
                List<UCell> BDLstF = pBDL.IEGetCellInHouse(h, 0x1FF).ToList();    //BDLstF:List of unresolved cells in the house
                int ncF = BDLstF.Count;
                if (ncF <= sz) continue;

                Combination cmbG = new Combination(ncF, sz);
                while (cmbG.Successor())
                {
                    BDLstF.ForEach(p => p.Selected = false);        // Initialize the cells in the BDLstF.
                    int selBlk = 0;
                    Array.ForEach(cmbG.Index, px => { (UC = BDLstF[px]).Selected = true; selBlk |= 1 << UC.b; });  //select cells by Combination

                    int noBSel = 0, noBNon = 0;
                    BDLstF.ForEach(P => {
                        if (P.Selected) noBSel |= P.FreeB;
                        else noBNon |= P.FreeB;
                    });

                    // int noBSel = BDLstF.Where(p=> p.Selected).Aggregate(0,(Q,q)=>Q|=q.FreeB);
                    // int noBNon = BDLstF.Where(p=>!p.Selected).Aggregate(0,(Q,q)=>Q|=q.FreeB);

                    if ((noBSel & noBNon) == 0) continue;                      //any digits that can be excluded?

                    //=============== Naked Locked Set ===============
                    if (!HiddenFlag)
                    {
                        SolData = h;
                        if (noBSel.BitCount() == sz)
                        {                          //Number of selected cell's digits is sz
                            if (h < 18 && selBlk.BitCount() == 1)
                            {
                                // When searching for a row or column and it is one block,
                                // there may be elements within the block that can be excluded.
                                int tfx2 = selBlk.BitToNum() + 18;              //bit expression -> House_No(18-26)
                                List<UCell> PLst = pBDL.IEGetCellInHouse(tfx2, noBSel).ToList();
                                BDLstF.ForEach(P => { if (P.Selected) PLst.Remove(P); });
                                if (PLst.Count > 0)
                                {
                                    SolData += tfx2 * 100;
                                    PLst.ForEach(P => { P.CancelB = P.FreeB & noBSel; });
                                }
                            }

                            resST = "";
                            foreach (var P in BDLstF)
                            {
                                if (P.Selected)
                                {
                                    P.Set_CellColorBkgColor_noBit(noBSel, AttCr, SolBkCr);
                                    resST += " " + P.rc.ToRCString();
                                }
                                else P.CancelB = P.FreeB & noBSel;
                            }
                            resST = resST.ToString_SameHouseComp() + " #" + noBSel.ToBitStringN(9);
                            _LockedSetResult(sz, resST, HiddenFlag);
                            if (__SimpleAnalyzerB__) return true;
                            if (!pAnMan.SnapSaveGP(pGP)) return true;
                        }
                    }

                    //=============== Hidden Locked Set ===============
                    if (HiddenFlag)
                    {
                        SolData = h;
                        if (noBNon.BitCount() == (ncF - sz))
                        {                    //Number of unselected cell's digits is (ncF-sz)
                            resST = "";
                            foreach (var P in BDLstF.Where(p => p.Selected))
                            {
                                P.CancelB = P.FreeB & noBNon;
                                P.Set_CellColorBkgColor_noBit(noBSel, AttCr, SolBkCr);
                                resST += " " + P.rc.ToRCString();
                            }
                            int nobR = noBSel.DifSet(noBNon);
                            resST = resST.ToString_SameHouseComp() + " #" + nobR.ToBitStringN(9);
                            _LockedSetResult(sz, resST, HiddenFlag);
                            if (__SimpleAnalyzerB__) return true;
                            if (!pAnMan.SnapSaveGP(pGP)) return true;
                        }
                    }
                }
            }
            return false;
        }

        private void _LockedSetResult(int sz, string resST, bool HiddenFlag)
        {
            string[] lockedSet_type = { "Pair[2D]", "Triple[3D]", "Quartet[4D]", "Set[5D]", "Set[6D]", "Set{7D}" };

            SolCode = 2;
            string SolMsg = "Locked" + lockedSet_type[sz - 2];
            if (HiddenFlag) SolMsg += " hidden";
            SolMsg += " " + resST;
            Result = SolMsg;
            ResultLong = SolMsg;
        }

        public bool LockedSetSub_old(int sz, bool HiddenFlag = false)
        {
            string resST = "";
            UCell UC;
            for (int h = 0; h < 27; h++)
            {
                List<UCell> BDLstF = pBDL.IEGetCellInHouse(h, 0x1FF).ToList();    //selecte cells in house
                int ncF = BDLstF.Count;
                if (ncF <= sz) continue;

                Combination cmbG = new Combination(ncF, sz);
                while (cmbG.Successor())
                {
                    BDLstF.ForEach(p => p.Selected = false);        //(Initialization of cells in the house. The target is several.)
                    int selBlk = 0;
                    Array.ForEach(cmbG.Index, p => { (UC = BDLstF[p]).Selected = true; selBlk |= 1 << UC.b; });  //selecte cells by Combination

                    int noBSel = 0, noBNon = 0;
                    BDLstF.ForEach(p => {
                        if (p.Selected) noBSel |= p.FreeB;
                        else noBNon |= p.FreeB;
                    });
                    if ((noBSel & noBNon) == 0) continue;                              //any digits that can be excluded?

                    //=============== Naked Locked Set ===============
                    if (!HiddenFlag)
                    {
                        if (noBSel.BitCount() == sz)
                        {                                  //Number of selected cell's digits is sz
                            if (h < 18 && selBlk.BitCount() == 1)
                            {
                                int tfx2 = selBlk.BitToNum() + 18;                      //bit expression -> House_No(18-26)
                                List<UCell> PLst = pBDL.IEGetCellInHouse(tfx2, noBSel).ToList();
                                BDLstF.ForEach(P => { if (P.Selected) PLst.Remove(P); });
                                if (PLst.Count > 0) PLst.ForEach(P => { P.CancelB = P.FreeB & noBSel; });
                            }

                            resST = "";
                            foreach (var P in BDLstF)
                            {
                                if (P.Selected)
                                {
                                    P.Set_CellColorBkgColor_noBit(noBSel, AttCr, SolBkCr);
                                    resST += " " + P.rc.ToRCString();
                                }
                                else P.CancelB = P.FreeB & noBSel;
                            }
                            resST = resST.ToString_SameHouseComp() + " #" + noBSel.ToBitStringN(9);
                            _LockedSetResult(sz, resST, HiddenFlag);
                            if (__SimpleAnalyzerB__) return true;
                            if (!pAnMan.SnapSaveGP(pGP)) return true;
                        }
                    }

                    //=============== Hidden Locked Set ===============
                    if (HiddenFlag)
                    {
                        if (noBNon.BitCount() == (ncF - sz))
                        {                          //Number of unselected cell's digits is (ncF-sz)
                            resST = "";
                            foreach (var P in BDLstF.Where(p => p.Selected))
                            {
                                P.CancelB = P.FreeB & noBNon;
                                P.Set_CellColorBkgColor_noBit(noBSel, AttCr, SolBkCr);
                                resST += " " + P.rc.ToRCString();
                            }
                            int nobR = noBSel.DifSet(noBNon);
                            resST = resST.ToString_SameHouseComp() + " #" + nobR.ToBitStringN(9);
                            _LockedSetResult(sz, resST, HiddenFlag);
                            if (__SimpleAnalyzerB__) return true;
                            if (!pAnMan.SnapSaveGP(pGP)) return true;
                        }
                    }
                }
            }
            return false;
        }

    }
}