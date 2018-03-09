using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName= "HighScoreTable", menuName ="HighScoreTable")]
public class HighScoreTable : ScriptableObject
{
    public List<string> keys;
    public List<TrackStats> values;

    public TrackStats GetOrCreate(string track)
    {
        int i = keys.IndexOf(track);

        if (i >= 0)
            return values[i];
        else
        {
            keys.Add(track);
            TrackStats t = new TrackStats();
            t.name = track;
            t.fastestLap = new HighScoreEntry();
            t.fastestTotal = new HighScoreEntry();
            t.totalPlayTime = 0f;

            values.Add(t);
            return t;
        }
    }

    public void UpdateHighScores()
    {
        TrackStats track = GetOrCreate(GameManager.instance.raceInfo.selectedTrack.sceneName);
        RaceInfo raceInfo = GameManager.instance.raceInfo;

        string lastName;
        float lastValue;

        if(raceInfo.fastestLap < track.fastestLap.valueInSeconds || track.fastestLap.valueInSeconds == 0)
        {
            lastName = track.fastestLap.name;
            lastValue = track.fastestLap.valueInSeconds;

            track.fastestLap.valueInSeconds = raceInfo.fastestLap;
            track.fastestLap.name = "Default";
            Debug.Log("NEW FASTEST LAP!");
            Debug.Log(string.Format("Last Fastest: {0} -{1}", raceInfo.parseTimeFromSeconds(lastValue), lastName));
        }
        if(raceInfo.totalSeconds < track.fastestTotal.valueInSeconds || track.fastestTotal.valueInSeconds == 0)
        {
            lastName = track.fastestTotal.name;
            lastValue = track.fastestTotal.valueInSeconds;

            track.fastestTotal.valueInSeconds = raceInfo.totalSeconds;
            track.fastestTotal.name = "Default";
            track.ghost = GameManager.instance.playerGhostData.trackData;

            Debug.Log("NEW FASTEST TOTAL!");
            Debug.Log(string.Format("Last Total: {0} -{1}", raceInfo.parseTimeFromSeconds(lastValue), lastName));
            
        }

        track.totalPlayTime += raceInfo.totalSeconds;
    }
}


