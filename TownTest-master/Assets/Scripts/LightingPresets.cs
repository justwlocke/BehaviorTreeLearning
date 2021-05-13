using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is from "Probably Spoonie's video at https://www.youtube.com/watch?v=m9hj9PdO328
// see also the original source code at https://www.youtube.com/redirect?event=video_description&redir_token=QUFFLUhqbnQtb3F0VFNVdUNNcmVSc1p6ejE5S0NsdkxfUXxBQ3Jtc0tucm85dTc3cGhIT0xkczBsMVlsQ2NVUDgySlZKNDZVMXRZVFVlZWtsRXhwbThQcHNSWDBoNmxLeXRyOFJRSVMxazlWdWlXWTVzdVZfWGJuMFB0QzI1M1phYTVzMlpkaVc1RDVNX0liWG1xWFRGTVh3aw&q=https%3A%2F%2Fgist.github.com%2FGlynn-Taylor%2F8ad1125ea7ef5aba1fa0374e80ac2c0d

[System.Serializable]
[CreateAssetMenu(fileName = "Lighting Preset", menuName = "Scriptables/Lighting Preset", order = 1)]
public class LightingPreset : ScriptableObject
{
    public Gradient AmbientColor;
    public Gradient DirectionalColor;
    public Gradient FogColor;
}