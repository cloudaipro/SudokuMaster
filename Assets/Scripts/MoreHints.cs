using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoreHints : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        GameSettings.Instance.Pause = true;
    }

    private void OnDisable()
    {
        
    }

    public void WatchAdToGetTwoFreeHints()
    {
        AdManager.Instance.ShowHintRewardAd();
    }

}
