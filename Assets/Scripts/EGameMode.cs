using System;
using System.ComponentModel;

[Serializable] public enum EGameMode {
    [Description("Not Set")]
    NOT_SET,
    [Description("Easy")]
    EASY,
    [Description("Medium")]
    MEDIUM,
    [Description("Hard")]
    HARD,
    [Description("Extreme")]
    EXTREME,
    [Description("Hell")]
    HELL
}
