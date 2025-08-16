using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SudokuNote : MonoBehaviour
{
    public GameObject Image;
    public Text Note;
    private bool _bSetted = false;
    public bool bSetted {
        get => _bSetted;
        set {
            _bSetted = value;
            gameObject.SetActive(value);
        }
    }
        
    private bool _bHighlight = false;
    public bool bHighlight {
        get => _bHighlight;
        set {
            _bHighlight = value;
            Image.GetComponent<Image>().color = (value) ? HighlightBGColor : Color.clear;
            Note.color = (value) ? HighlightTextColor : TextColor;
        }
    }

    private bool _bAlarm = false;
    public bool bAlarm
    {
        get => _bAlarm;
        set
        {
            _bAlarm = value;
            Image.GetComponent<Image>().color = (value) ? AlarmBGColor : Color.clear;
            Note.color = (value) ? HighlightTextColor : TextColor;
        }
    }

    public Color HighlightBGColor = Color.clear;
    public Color AlarmBGColor = Color.red;
    public Color TextColor = Color.gray;
    public Color HighlightTextColor = Color.white;

    public override string ToString()
    {
        return _bSetted ? Note.text : " ";
    }
}
