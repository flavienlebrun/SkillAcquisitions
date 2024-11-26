using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/**
Classe : AudioBox
Système à base de piste qui permet à un GameObject de plus facilement gérer un ensemble d'AudioSources
*/

public class AudioBox : MonoBehaviour
{
    [SerializeField]
    private bool hasOneShotTrack = false;
    [SerializeField]
    private int nbLoopTrack = 0;
    [SerializeField]
    private bool is2D = false;

    private GameObject audioSource;

    private AudioSource oneShotSource;
    private AudioSource[] loopSources;

    private void Awake () {
        audioSource = new GameObject("AudioSource");
        audioSource.transform.position = transform.position;
        audioSource.transform.parent = transform;

        if(hasOneShotTrack){
            oneShotSource = audioSource.AddComponent<AudioSource>();
            oneShotSource.spatialBlend = is2D ? 0 : 1;
            oneShotSource.maxDistance = 30;

            Keyframe[] keys = new Keyframe[6];
            keys[0].time = 1; keys[0].value = 1;
            keys[1].time = 2; keys[1].value = 0.5f;
            keys[2].time = 4; keys[2].value = 0.25f;
            keys[3].time = 8; keys[3].value = 0.125f;
            keys[4].time = 16; keys[4].value = 0.0625f;
            keys[5].time = 30; keys[5].value = 0;
            AnimationCurve curve = new AnimationCurve(keys);
            curve.SmoothTangents(0, 0);
            curve.SmoothTangents(1, 0);
            curve.SmoothTangents(2, 0);
            curve.SmoothTangents(3, 0);
            curve.SmoothTangents(4, 0);
            curve.SmoothTangents(5, 0);
            oneShotSource.rolloffMode = AudioRolloffMode.Custom;
            oneShotSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
        }

        loopSources = new AudioSource[nbLoopTrack];
        for(int i = 0; i < nbLoopTrack; ++i){
            loopSources[i] = audioSource.AddComponent<AudioSource>();
            loopSources[i].loop = true;
            loopSources[i].spatialBlend = is2D ? 0 : 1;
        }
    }

    public void PlayOneShot (SoundOneShot sound, float volumeFactor = 1) {
        if(!hasOneShotTrack){
            Debug.LogWarning("This AudioBox do not have a OneShotTrack, " + sound + " cannot be played");
            return;
        }

        AudioManager.Instance.PlayOneShot(sound, oneShotSource);
    }

    public void SetOneShotPitch (float pitch) {
        oneShotSource.pitch = pitch;
    }

    public void PlayLoop (SoundLoop sound, float volumeFactor = 1) {
        AudioSource source = null;
        for(int i = 0; i < nbLoopTrack; ++i){
            AudioSource src = loopSources[i];
            if(!src.isPlaying){
                source = src;
                break;
            }
        }

        if(!source){
            Debug.LogWarning("This AudioBox do not have enough LoopTrack, " + sound + " cannot be played");
            return;
        }

        AudioManager.Instance.PlayLoop(sound, source);
    }

    public void StopLoop (SoundLoop sound) {
        AudioSource source = loopSources.FirstOrDefault<AudioSource>(src => AudioManager.Instance.IsSameSoundLoop(sound, src.clip));
        if(!source){
            Debug.LogWarning("This AudioBox do not play the sound : " + sound + ", so they cannot be stopped");
            return;
        }

        if(source.isPlaying) source.Stop();
        source.clip = null;
    }

    public void SetLoopVolume (SoundLoop sound, float volume) {
        AudioSource source = loopSources.FirstOrDefault<AudioSource>(src => AudioManager.Instance.IsSameSoundLoop(sound, src.clip));
        if(!source){
            Debug.LogWarning("This AudioBox do not play the sound : " + sound + ", so they cannot change volume");
            return;
        }

        AudioManager.Instance.SetLoopVolume(sound, source, volume);
    }

    public void PlayLoopForTime (SoundLoop sound, float time, float volumeFactor = 1) {
        PlayLoop(sound, volumeFactor);
        StartCoroutine(StopLoopAfterTime(sound, time));
    }

    private IEnumerator StopLoopAfterTime (SoundLoop sound, float time) {
        yield return new WaitForSeconds(time);
        StopLoop(sound);
    }

    public void StopAll () {
        for(int i = 0; i < nbLoopTrack; ++i){
            AudioSource source = loopSources[i];
            if(source.isPlaying) source.Stop();
            source.clip = null;
        }
    }

}
