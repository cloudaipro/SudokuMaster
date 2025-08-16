using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using GIDOO_space;

namespace GNPXcore
{

    // First, understand Bit81, UCell, ConnectedCells, HouseCells, and IEGetCellInHouse.
    //  https://gidoo-code.github.io/Sudoku_Solver_Generator/page23.html

    // Then the following algorithm("Single") is almost trivial.
    //  https://gidoo-code.github.io/Sudoku_Solver_Generator/page31a.html
    //  https://gidoo-code.github.io/Sudoku_Solver_Generator/page31b.html
    //  https://gidoo-code.github.io/Sudoku_Solver_Generator/page31c.html


    public partial class SimpleSingleGen : AnalyzerBaseV2
    {
        //*==*==*==*==* Last Digit *==*==*==*==*==*==*==*==* 
        public bool LastDigit()
        {
            bool SolFound = false;
            for (int h = 0; h < 27; h++)
            { //h:house (row:0-, column:9-, block:18-)
                if (pBDL.IEGetCellInHouse(h, 0x1FF).Count() == 1)
                {   //// only one element(digit) in house

                    //---------------------- found
                    SolFound = true;
                    var P = pBDL.IEGetCellInHouse(h, 0x1FF).First();
                    P.FixedNo = P.FreeB.BitToNum() + 1;
                    if (!chbConfirmMultipleCells) goto LFound;
                }
            }

        LFound:
            if (SolFound)
            {
                SolCode = 1;
                Result = "Last Digit";
                if (__SimpleAnalyzerB__) return true;
                if (SolInfoB) ResultLong = Result;
                pAnMan.SnapSaveGP(pGP);
                return true;
            }
            return false;
        }

        //*==*==*==*==* Two Dir Single *==*==*==*==*==*==*==*==*
        public bool TwoDirSingle()
        {
            bool SolFound = false;
            IEnumerable<UCell> cellsInHouse;
            UCell candicatedCell;
            bool bAllHaveTheNumbert = true;
            for (int no = 0; no < 9; no++)
            { //no:digit
                int noB = 1 << no;
                for (int h = 18; h < 27; h++)
                {
                    cellsInHouse = pBDL.IEGetCellInHouse(h, noB);
                    if (cellsInHouse.Count() == 1)
                    {  //only one cell in house(h)
                        candicatedCell = cellsInHouse.First();
                        bAllHaveTheNumbert = true;
                        try
                        {
                            int r = 0, c = 0, tp = h / 9, fx = h % 9;
                            for (int nx = 0; nx < 9; nx++)
                            {
                                r = (fx / 3) * 3 + nx / 3; c = (fx % 3) * 3 + nx % 3;
                                if (r == candicatedCell.r && c == candicatedCell.c) continue;
                                if (pBDL[r * 9 + c].No == 0)
                                {
                                    if (!pBDL.IsExistNumberInHouse(r, no + 1) && !pBDL.IsExistNumberInHouse(c + 9, no + 1))
                                    {
                                        bAllHaveTheNumbert = false;
                                        break;
                                    }
                                }
                            }
                            if (bAllHaveTheNumbert)
                            {
                                SolFound = true;
                                candicatedCell.FixedNo = no + 1;
                                goto LFound;
                            }
                        }
                        catch (Exception e) { WriteLine($"{e.Message}\r{e.StackTrace}"); }
                    }
                }
            }

        LFound:
            if (SolFound)
            {
                SolCode = 1;
                Result = "Two Dir Single";
                if (__SimpleAnalyzerB__) return true;
                if (SolInfoB) ResultLong = "Two Dir Single";
                pAnMan.SnapSaveGP(pGP);
                return true;
            }
            return false;
        }
        
        //*==*==*==*==* Last Digit *==*==*==*==*==*==*==*==* 
        public bool MyLastDigit()
        {
            bool SolFound = false;
            IList<UCell> cellsInHouse;
            for (int h = 0; h < 27; h++)
            {   //h:house (row:0-, column:9-, block:18-)
                cellsInHouse = pBDL.MyGetCellWithFreeBInHouse(h, 0x1FF);
                if (cellsInHouse.Count() == 1)
                {   //// only one element(digit) in house

                    //---------------------- found
                    SolFound = true;
                    var P = cellsInHouse.First();
                    P.FixedNo = P.FreeB.BitToNum() + 1;
                    if (!chbConfirmMultipleCells) goto LFound;
                }
            }

        LFound:
            if (SolFound)
            {
                SolCode = 1;
                Result = "Last Digit";
                if (__SimpleAnalyzerB__) return true;
                if (SolInfoB) ResultLong = Result;
                pAnMan.SnapSaveGP(pGP);
                return true;
            }
            return false;
        }

        //*==*==*==*==* Two Dir Single *==*==*==*==*==*==*==*==*
        private bool CheckHouseTwoDirSingle(int[] hArry)
        {
            IList<UCell> cellsInHouse;
            UCell candicatedCell;
            bool bAllHaveTheNumbert = true;
            (int r, int c) rc;
            UCell targetCell;
            int anotherH1, anotherH2;
            int nx = 0;
            for (int no = 0; no < 9; no++)
            { //no:digit
                int noB = 1 << no;
                foreach (int h in hArry)
                {
                    cellsInHouse = pBDL.MyGetCellWithFreeBInHouse(h, noB);
                    if (cellsInHouse.Count() == 1)
                    {  //only one cell in house(h)
                        candicatedCell = cellsInHouse.First();
                        bAllHaveTheNumbert = true;
                        try
                        {
                            for (nx = 0; nx < 9; nx++)
                            {
                                rc = MyStaticSA.HPosToRC(h, nx);
                                if (rc.r == candicatedCell.r && rc.c == candicatedCell.c) continue;
                                targetCell = pBDL[rc.r * 9 + rc.c];
                                if (targetCell.No == 0)
                                {
                                    if (h < 9) // row
                                    {
                                        anotherH1 = targetCell.b + 18; // block
                                        anotherH2 = targetCell.c + 9;  // col
                                    }
                                    else if (h < 18) // col
                                    {
                                        anotherH1 = targetCell.b + 18; // block
                                        anotherH2 = targetCell.r;      // row
                                    }
                                    else
                                    {
                                        anotherH1 = targetCell.r;      // row
                                        anotherH2 = targetCell.c + 9;  // col
                                    }
                                    if (!pBDL.IsExistNumberInHouse(anotherH1, no + 1) && !pBDL.IsExistNumberInHouse(anotherH2, no + 1))
                                    {
                                        bAllHaveTheNumbert = false;
                                        break;
                                    }
                                }
                            }
                            if (bAllHaveTheNumbert)
                            {
                                candicatedCell.FixedNo = no + 1;
                                candicatedCell.houseIndex = h;
                                return true;
                            }
                        }
                        catch (Exception e) { WriteLine($"{e.Message}\r{e.StackTrace}"); }
                    }
                }
            }
            return false;
        }
        public bool MyTwoDirSingle()
        {
            if (CheckHouseTwoDirSingle(Enumerable.Range(18, 9).ToArray()) ||
                CheckHouseTwoDirSingle(Enumerable.Range(0, 18).ToArray()))
            {
                SolCode = 1;
                Result = "Two Dir Single";
                if (__SimpleAnalyzerB__) return true;
                if (SolInfoB) ResultLong = "Two Dir Single";
                pAnMan.SnapSaveGP(pGP);
                return true;
            }
            return false;
        }

        //*==*==*==*==* Hidden Single *==*==*==*==*==*==*==*==*
        public bool MyHiddenSingle()
        {
            bool SolFound = false;
            IList<UCell> cellsInHouse;
            for (int no = 0; no < 9; no++)
            { //no:digit
                int noB = 1 << no;
                for (int h = 0; h < 27; h++)
                {
                    cellsInHouse = pBDL.MyGetCellWithFreeBInHouse(h, noB);
                    if (cellsInHouse.Count == 1 && cellsInHouse.First().FreeBC > 1)
                    {  //only one cell in house(h)
                        try
                        {
                            //---------------------- found
                            SolFound = true;
                            var P = cellsInHouse.First();
                            P.FixedNo = no + 1;
                            P.houseIndex = h;
                            if (!chbConfirmMultipleCells) goto LFound;
                        }
                        catch (Exception e) { WriteLine($"{e.Message}\r{e.StackTrace}"); }
                    }
                }
            }

        LFound:
            if (SolFound)
            {
                SolCode = 1;
                Result = "Hidden Single";
                if (__SimpleAnalyzerB__) return true;
                if (SolInfoB) ResultLong = "Hidden Single";
                pAnMan.SnapSaveGP(pGP);
                return true;
            }
            return false;
        }
    }
}