using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartButton : MonoBehaviour
{
    public GameObject restartUI;
    public WaveSystem waveSystem;
    public TimeScaleToggle timeScaleToggle;
    public void OnRestartButtonClick()
    {
        timeScaleToggle.RestartTime();
        restartUI.SetActive(false);
        waveSystem.AgainWaveCharacterChanged();
    }
}
