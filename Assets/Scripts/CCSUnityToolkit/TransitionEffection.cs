using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TransitionEffection
{
    //public delegate void DidBlockGradient(object gameObject);
    public static IEnumerator BlockGradientTransition<T>(IList<T[]> finishedList, List<Color> gradient_colors, float transitionTime
                                                         //, DidBlockGradient didTransitioned = null
                                                        ) where T : Selectable
    {
        // Get the size of the gradient color list, the size of the cell array, and initialize variables
        int gradientSize = gradient_colors.Count;
        int gradientCycleSize = gradientSize * 2;
        int cellArraySize = finishedList[0].Count();
        float timer = 0;
        List<Color[]> startColors = new List<Color[]>();
        finishedList.ForEach(x => startColors.Add(x.Select(y => y.colors.disabledColor).ToArray()));
        float unitTime = transitionTime / (cellArraySize + gradientCycleSize);
        int timeIdx = 0;
        int cellIdx = 0;
        int forceIdx = 0;
        int gradientIdx = 0;
        Color fromColor;
        Color toColor;
        ColorBlock cellColor;
        int prevTimeIdx = -1;
        while (timer < transitionTime)
        {
            // Increment timer and calculate the current time index
            timer += Time.deltaTime;

            yield return null;

            timeIdx = (int)(timer / unitTime);

            // Check whether the time index has changed since the previous iteration
            if (prevTimeIdx != -1 && prevTimeIdx != timeIdx)
            {
                // Loop through cells and gradient colors to update the colors of cells that have already finished transitioning
                for (forceIdx = prevTimeIdx; forceIdx < timeIdx; forceIdx++)
                {
                    for (cellIdx = forceIdx, gradientIdx = 0; cellIdx >= 0 && gradientIdx < gradientCycleSize; cellIdx--, gradientIdx++)
                    {
                        if (cellIdx < cellArraySize)
                        {
                            // Loop through each finished cell array and update the color of the current cell
                            finishedList.ForEachWithIndex((cellArray, finishedIdx) =>
                            {
                                // Determine the to color based on the current gradient index and whether the cell has already finished transitioning
                                toColor = (gradientIdx < gradientSize) ? gradient_colors[gradientIdx] :
                                            (gradientIdx < gradientCycleSize - 1) ? gradient_colors[(gradientCycleSize - 1) - gradientIdx - 1] : startColors[finishedIdx][cellIdx];
                                // Update the disabled color of the current cell
                                cellColor = cellArray[cellIdx].colors;
                                cellColor.disabledColor = toColor;
                                cellArray[cellIdx].colors = cellColor;
                                //didTransitioned?.Invoke(cellArray[cellIdx]);
                            });
                        }
                    }
                }
            }
            // Loop through cells and gradient colors to update the colors of cells that are currently transitioning
            for (cellIdx = timeIdx, gradientIdx = 0; cellIdx >= 0 && gradientIdx < gradientCycleSize; cellIdx--, gradientIdx++)
            {
                if (cellIdx < cellArraySize)
                {
                    // Loop through each finished cell array and update the color of the current cell
                    finishedList.ForEachWithIndex((cellArray, finishedIdx) =>
                    {
                        // Determine the from and to colors based on the current gradient index and whether the cell has already finished transitioning
                        fromColor = (gradientIdx == 0) ? startColors[finishedIdx][cellIdx] :
                                (gradientIdx <= gradientSize) ? gradient_colors[gradientIdx - 1] :
                                (gradientIdx < gradientCycleSize) ? gradient_colors[(gradientCycleSize - 1) - gradientIdx] : startColors[finishedIdx][cellIdx];
                        toColor = (gradientIdx < gradientSize) ? gradient_colors[gradientIdx] :
                                            (gradientIdx < gradientCycleSize - 1) ? gradient_colors[(gradientCycleSize - 1) - gradientIdx - 1] : startColors[finishedIdx][cellIdx];

                        cellColor = cellArray[cellIdx].colors;
                        cellColor.disabledColor = Color.Lerp(fromColor, toColor, (timer % unitTime / unitTime));
                        cellArray[cellIdx].colors = cellColor;
                    });
                }
            }
            if (prevTimeIdx != timeIdx)
                prevTimeIdx = timeIdx;
        }
    }

    public static IEnumerator BlockFadeInGradientTransition<T>(IList<T[]> finishedList,
        IList<Graphic[]> graphicObjs, List<Color> gradient_colors, float transitionTime
                                                        //, DidBlockGradient didTransitioned = null
                                                        ) where T : Selectable
    {
        // Get the size of the gradient color list, the size of the cell array, and initialize variables
        int gradientSize = gradient_colors.Count;
        int gradientCycleSize = gradientSize * 2;
        int cellArraySize = finishedList[0].Count();
        float timer = 0;
        List<Color[]> startColors = new List<Color[]>();
        finishedList.ForEach(x => startColors.Add(x.Select(y => y.colors.disabledColor).ToArray()));

        float[] fadeInColor = Enumerable.Range(1, gradientCycleSize).Select(x => 1f / x).Reverse().ToArray();

        float unitTime = transitionTime / (cellArraySize + gradientCycleSize);
        //float fadeInTime = unitTime * gradientCycleSize;
        int timeIdx = 0;
        int prevTimeIdx = -1;

        int cellIdx = 0;
        int forceIdx = 0;
        int gradientIdx = 0;
        Color fromColor;
        Color toColor;
        ColorBlock cellColor;

        Color c;
        graphicObjs.ForEach(gs => gs.ForEach(g =>
        {
            c = g.color;
            c.a = 0;
            g.color = c;
        }));

        while (timer < transitionTime)
        {
            // Increment timer and calculate the current time index
            timer += Time.deltaTime;

            yield return null;

            timeIdx = (int)(timer / unitTime);
            
            // Check whether the time index has changed since the previous iteration
            if (prevTimeIdx >= 0 && prevTimeIdx != timeIdx)
            {
                // Loop through cells and gradient colors to update the colors of cells that have already finished transitioning
                for (forceIdx = prevTimeIdx; forceIdx < timeIdx; forceIdx++)
                {
                    for (cellIdx = forceIdx, gradientIdx = 0; cellIdx >= 0 && gradientIdx < gradientCycleSize; cellIdx--, gradientIdx++)
                    {
                        if (cellIdx < cellArraySize)
                        {
                            // Loop through each finished cell array and update the color of the current cell
                            finishedList.ForEachWithIndex((cellArray, finishedIdx) =>
                            {
                                // Determine the to color based on the current gradient index and whether the cell has already finished transitioning
                                toColor = (gradientIdx < gradientSize) ? gradient_colors[gradientIdx] :
                                            (gradientIdx < gradientCycleSize - 1) ? gradient_colors[(gradientCycleSize - 1) - gradientIdx - 1] : startColors[finishedIdx][cellIdx];
                                // Update the disabled color of the current cell
                                cellColor = cellArray[cellIdx].colors;
                                cellColor.disabledColor = toColor;
                                cellArray[cellIdx].colors = cellColor;                                

                                graphicObjs[finishedIdx][cellIdx].Also(g =>
                                {
                                    toColor = g.color;
                                    toColor.a = 1;
                                    g.color = toColor;                                    
                                });
                            });
                        }
                    }
                }
            }
            // Loop through cells and gradient colors to update the colors of cells that are currently transitioning
            for (cellIdx = timeIdx, gradientIdx = 0; cellIdx >= 0 && gradientIdx < gradientCycleSize; cellIdx--, gradientIdx++)
            {
                if (cellIdx < cellArraySize)
                {
                    // Loop through each finished cell array and update the color of the current cell
                    finishedList.ForEachWithIndex((cellArray, finishedIdx) =>
                    {
                        // Determine the from and to colors based on the current gradient index and whether the cell has already finished transitioning
                        fromColor = (gradientIdx == 0) ? startColors[finishedIdx][cellIdx] :
                                (gradientIdx <= gradientSize) ? gradient_colors[gradientIdx - 1] :
                                (gradientIdx < gradientCycleSize) ? gradient_colors[(gradientCycleSize - 1) - gradientIdx] : startColors[finishedIdx][cellIdx];
                        toColor = (gradientIdx < gradientSize) ? gradient_colors[gradientIdx] :
                                            (gradientIdx < gradientCycleSize - 1) ? gradient_colors[(gradientCycleSize - 1) - gradientIdx - 1] : startColors[finishedIdx][cellIdx];

                        cellColor = cellArray[cellIdx].colors;
                        cellColor.disabledColor = Color.Lerp(fromColor, toColor, (timer % unitTime / unitTime));
                        cellArray[cellIdx].colors = cellColor;
                        
                        if (gradientIdx < gradientCycleSize - 1)
                            graphicObjs[finishedIdx][cellIdx].Also(g =>
                            {
                                fromColor = g.color;
                                fromColor.a = fadeInColor[gradientIdx];
                                toColor = g.color;
                                toColor.a = (fadeInColor[gradientIdx + 1]);
                                g.color = Color.Lerp(fromColor, toColor, (timer % unitTime / unitTime));                               
                            });                        
                    });                    
                }
            }
            if (prevTimeIdx != timeIdx)
                prevTimeIdx = timeIdx;
        }
    }
}
