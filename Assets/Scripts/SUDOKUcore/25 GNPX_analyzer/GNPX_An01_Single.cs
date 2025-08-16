using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;
using GIDOO_space;
using UnityEngine;

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
        public SimpleSingleGen(GNPX_AnalyzerMan pAnMan) : base(pAnMan) { }

        //*==*==*==*==* Naked Single *==*==*==*==*==*==*==*==* 
        public bool NakedSingle()
        {
            bool SolFound = false;
            foreach (UCell P in pBDL.Where(p => p.FreeBC == 1))
            {   // only one element(digit) in cell

                //---------------------- found
                SolFound = true;
                P.FixedNo = P.FreeB.BitToNum() + 1;
                if (!chbConfirmMultipleCells) goto LFound;
            }

        LFound:
            if (SolFound)
            {
                SolCode = 1;
                Result = "Naked Single";
                if (__SimpleAnalyzerB__) return true;
                if (SolInfoB) ResultLong = "Naked Single";
                pAnMan.SnapSaveGP(pGP);
                return true;
            }
            return false;
        }

        //*==*==*==*==* Hidden Single *==*==*==*==*==*==*==*==*
        public bool HiddenSingle()
        {
            bool SolFound = false;
            for (int no = 0; no < 9; no++)
            { //no:digit
                int noB = 1 << no;
                for (int h = 0; h < 27; h++)
                {
                    if (pBDL.IEGetCellInHouse(h, noB).Count() == 1)
                    {  //only one cell in house(h)
                        try
                        {
                            var PLst = pBDL.IEGetCellInHouse(h, noB).Where(Q => Q.FreeBC > 1);
                            if (PLst.Count() <= 0) continue; // => NakedSingle

                            //---------------------- found
                            SolFound = true;
                            var P = PLst.First();
                            P.FixedNo = no + 1;
                            P.houseIndex = h; //alex
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