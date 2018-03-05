using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RaceInfo
{
    [Tooltip("The Current Selected Track to be Run.")]
    public TrackAsset selectedTrack;

    public GameObject selectedCar;
    [ReadOnly]
    public int currentLap;// 0 based index!!
    [ReadOnly]
    public float[] lapTimes;
    [ReadOnly]
    public float currentLapTime;

    public int numberOfLaps;

    public float totalSeconds
    {
        get
        {
            float total = currentLapTime;
            for (int i = 0; i < currentLap; i++)
            {
                total += lapTimes[i];
            }
            return total;
        }
    }

    public float averageSecondsPerLap
    {
        get
        {
            return totalSeconds / (currentLap > 0 ? currentLap : currentLap + 1);
        }
    }

    public float fastestLap
    {
        get
        {
            float fastest = 0;
            for(int i = 0; i <= currentLap - 1; i++)
            {
                if(fastest > lapTimes[i] || fastest == 0)
                {
                    fastest = lapTimes[i];
                }
            }
            return fastest;
        }
    }

    public void logStats()
    {
        Debug.Log(string.Format("Total: {0}", parseTimeFromSeconds(totalSeconds)));
        Debug.Log(string.Format("Average: {0}", parseTimeFromSeconds(averageSecondsPerLap)));
        Debug.Log(string.Format("Fastest: {0}", parseTimeFromSeconds(fastestLap)));
        
    }

     public string parseTimeFromSeconds (float seconds)
    {
        int min = (int)seconds / 60;
        float sec = seconds - (min * 60);

        return string.Format("{0}:{1}", min, sec);
    }
}
