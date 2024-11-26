using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioFiles", menuName = "Audio/AudioFiles", order = 2)]
public class AudioFiles : ScriptableObject
{
    [SerializeField]
    private SoundLoopClip roomSoundEffect = null;

    [SerializeField]
    private SingleSoundOneShotClip cryPain = null;

 
    public SoundOneShotClip SoundOneShotToClip (SoundOneShot sound) 
    {
        switch (sound) 
        {
            case SoundOneShot.CryPain : return cryPain;

            default : 
                Debug.LogError("SoundOneShotClip : " + sound + " was not found!");
                return null;
        }
    }

    public SoundLoopClip SoundLoopToClip (SoundLoop sound) 
    {
        switch (sound) 
        {
            case SoundLoop.RoomSoundEffect : return roomSoundEffect;

            default : 
                Debug.LogError("SoundLoopClip : " + sound + " was not found!");
                return null;
        }
    }
}

public enum SoundOneShot 
{
    CryPain
}

public enum SoundLoop 
{
    RoomSoundEffect
}
