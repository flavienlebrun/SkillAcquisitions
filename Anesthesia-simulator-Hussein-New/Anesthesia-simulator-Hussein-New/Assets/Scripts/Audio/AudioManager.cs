using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
Classe : AudioManager
Classe qui permet le bon fonctionnement des différents AudioBox de la scène,
En faisant le lien avec AudioFiles
*/

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance {get; private set;}

    [SerializeField]
    private AudioFiles audioFiles = null;
    [SerializeField]
    [Range(0, 1)]
    private float globalVolume = 1;

    private void Awake() {
		if (Instance) {
			Destroy(this);
            return;
		}

        Instance = this;
	}

    private void OnDestroy () {
        if(Instance == this) Instance = null;
    }

    public void PlayOneShot (SoundOneShot sound, AudioSource source, float volumeFactor = 1) {
        SoundOneShotClip c = audioFiles.SoundOneShotToClip(sound);
        source.PlayOneShot(c.Clip, c.Volume * volumeFactor * globalVolume);
    }

    public void PlayLoop (SoundLoop sound, AudioSource source, float volumeFactor = 1) {
        SoundLoopClip c = audioFiles.SoundLoopToClip(sound);
        source.clip = c.Clip;
        source.volume = c.Volume * volumeFactor * globalVolume;
        source.Play();
    }

    public void SetLoopVolume (SoundLoop sound, AudioSource source, float volume) {
        SoundLoopClip c = audioFiles.SoundLoopToClip(sound);
        source.volume = c.Volume * volume * globalVolume;
    }

    public bool IsSameSoundLoop (SoundLoop sound, AudioClip clip) {
        return audioFiles.SoundLoopToClip(sound).Clip == clip;
    }
}
