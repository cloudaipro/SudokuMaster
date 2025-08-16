using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonUtils 
{
    public static string FormatDeltaTime(float delta_time)
    {
        var span = TimeSpan.FromSeconds(delta_time);
        if (span.Hours == 0)
            return String.Format("{0}:{1}", span.Minutes.LeadingZero(2), span.Seconds.LeadingZero(2));
        else
            return String.Format("{0}:{1}:{2}", span.Hours.LeadingZero(2), span.Minutes.LeadingZero(2), span.Seconds.LeadingZero(2));

    }
}
