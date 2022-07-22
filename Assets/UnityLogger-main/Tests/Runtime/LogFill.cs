using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StaticHexLog;

public class LogFill : MonoBehaviour
{
    int countUpMax = 2;
    int countUp = 0;

    //This 
    void Update()
    {
        countUp = countUp + 1;
        if (countUp >= 1)
        {
            countUp = 0;
            Info("Garbage logs to test rotation");
        }
    }
}
