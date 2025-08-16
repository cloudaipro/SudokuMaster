using System;
using System.Collections.Generic;
using System.Linq;
using static System.Diagnostics.Debug;

//using System.Drawing;
using GIDOO_space;

namespace GNPXcore
{
    using SWMColor = UnityEngine.Color;

    //========== Extended Function ==========
    static public class MyStaticSA
    {
        static public (int h1, int h2, int h3, int h4) GetExcludeHouse(int h, int nx)
        {
            (int h1, int h2, int h3, int h4) tmp;
            var rc = HPosToRC(h, nx);
            var tp = h / 9;
            var b = ((int)(rc.r / 3)) * 3 + ((int)(rc.c / 3));
            switch (tp)
            {
                case 0: //row(h:0-8)
                    switch (rc.c % 3)
                    {
                        case 0:
                            tmp.h1 = rc.c + 1 + 9;
                            tmp.h2 = rc.c + 2 + 9;
                            break;
                        case 1:
                            tmp.h1 = rc.c - 1 + 9;
                            tmp.h2 = rc.c + 1 + 9;
                            break;
                        default:
                            tmp.h1 = rc.c - 2 + 9;
                            tmp.h2 = rc.c - 1 + 9;
                            break;
                    }
                    switch (b % 3)
                    {
                        case 0:
                            tmp.h3 = b + 1 + 18;
                            tmp.h4 = b + 2 + 18;
                            break;
                        case 1:
                            tmp.h3 = b - 1 + 18;
                            tmp.h4 = b + 1 + 18;
                            break;
                        default:
                            tmp.h3 = b - 2 + 18;
                            tmp.h4 = b - 1 + 18;
                            break;
                    }                    
                    break;
                case 1: //column(h:9-17)
                    switch (rc.r % 3)
                    {
                        case 0:
                            tmp.h1 = rc.r + 1;
                            tmp.h2 = rc.r + 2;
                            break;
                        case 1:
                            tmp.h1 = rc.r - 1;
                            tmp.h2 = rc.r + 1;
                            break;
                        default:
                            tmp.h1 = rc.r - 2;
                            tmp.h2 = rc.r - 1;
                            break;
                    }
                    switch (b / 3)
                    {
                        case 0:
                            tmp.h3 = b + 3 + 18;
                            tmp.h4 = b + 6 + 18;
                            break;
                        case 1:
                            tmp.h3 = b - 3 + 18;
                            tmp.h4 = b + 3 + 18;
                            break;
                        default:
                            tmp.h3 = b - 6 + 18;
                            tmp.h4 = b - 3 + 18;
                            break;
                    }
                    break;
                default: //block(h:18-26)
                    switch (rc.r % 3)
                    {
                        case 0:
                            tmp.h1 = rc.r + 1;
                            tmp.h2 = rc.r + 2;
                            break;
                        case 1:
                            tmp.h1 = rc.r - 1;
                            tmp.h2 = rc.r + 1;
                            break;
                        default:
                            tmp.h1 = rc.r - 2;
                            tmp.h2 = rc.r - 1;
                            break;
                    }
                    switch (rc.c % 3)
                    {
                        case 0:
                            tmp.h3 = rc.c + 1 + 9;
                            tmp.h4 = rc.c + 2 + 9;
                            break;
                        case 1:
                            tmp.h3 = rc.c - 1 + 9;
                            tmp.h4 = rc.c + 1 + 9;
                            break;
                        default:
                            tmp.h3 = rc.c - 2 + 9;
                            tmp.h4 = rc.c - 1 + 9;
                            break;
                    }                    
                    break;
            }
            return tmp;

        }
        static public (int r, int c) HPosToRC(int h, int nx)
        {
            int r = 0, c = 0, tp = h / 9, fx = h % 9;
            switch (tp)
            {
                case 0: r = fx; c = nx; break; //row(h:0-8)
                case 1: r = nx; c = fx; break; //column(h:9-17)
                case 2: r = (fx / 3) * 3 + nx / 3; c = (fx % 3) * 3 + nx % 3; break; //block(h:18-26)
            }
            return (r, c);
        }
        static public IList<UCell> MyGetCellWithFreeBInHouse(this List<UCell> pBDL, int h, int FreeB = 0x1FF)
        {
            var tmpList = new List<UCell>();
            //Find the cells with digits X(FreeB) in house.
            int r = 0, c = 0, tp = h / 9, fx = h % 9;
            for (int nx = 0; nx < 9; nx++)
            {
                switch (tp)
                {
                    case 0: r = fx; c = nx; break; //row(h:0-8)
                    case 1: r = nx; c = fx; break; //column(h:9-17)
                    case 2: r = (fx / 3) * 3 + nx / 3; c = (fx % 3) * 3 + nx % 3; break; //block(h:18-26)
                }
                UCell P = pBDL[r * 9 + c];
                P.nx = nx;
                if ((P.FreeB & FreeB) > 0) tmpList.Add(P);
            }
            return tmpList;
        }
        static public bool IsExistNumberInHouse(this List<UCell> pBDL, int h, int No)
        {
            //Find the cells with digits X(FreeB) in house.
            int r = 0, c = 0, tp = h / 9, fx = h % 9;
            for (int nx = 0; nx < 9; nx++)
            {
                switch (tp)
                {
                    case 0: r = fx; c = nx; break; //row(h:0-8)
                    case 1: r = nx; c = fx; break; //column(h:9-17)
                    case 2: r = (fx / 3) * 3 + nx / 3; c = (fx % 3) * 3 + nx % 3; break; //block(h:18-26)
                }
                UCell P = pBDL[r * 9 + c];
                if (P.No == No || P.No == (No * -1)) return true;
            }
            return false;
        }

        static public List<int> HouseToNumList(this int HH)
        {
            List<int> st = new List<int>();
            if ((HH & 0x1FF) > 0) st.AddRange((HH & 0x1FF).BitToNumList());
            if (((HH >>= 9) & 0x1FF) > 0) st.AddRange((HH & 0x1FF).BitToNumList().Select(x => x + 9));
            if (((HH >>= 9) & 0x1FF) > 0) st.AddRange((HH & 0x1FF).BitToNumList().Select(x => x + 18));
            return st;
        }


    }
}