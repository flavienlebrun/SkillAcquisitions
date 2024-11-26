using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/**
Classe : SoundClip
Représente un AudioClip ainsi que sa configuration pour l'AudioSource le jouant
*/

[System.Serializable]
public abstract class SoundClip {
    public abstract AudioClip Clip {get;}

    [SerializeField]
    [Range(0, 1)]
    private float volume = 1;
    public float Volume {get{return volume;}}

    [SerializeField]
    private AudioMixer mixer = null;
    public bool Mixer {get{return mixer;}}
}

/**
Classe : SoundClip
Dérivé de SoundClip, représente un son qui ne se joue qu'une fois
*/

[System.Serializable]
public abstract class SoundOneShotClip : SoundClip {}

/**
Classe : SoundLoopClip
Dérivé de SoundClip, représente un son qui ne se joue en boucle
*/

[System.Serializable]
public class SoundLoopClip : SoundClip {
    [SerializeField]
    private AudioClip clip = null;
    public override AudioClip Clip {get{return clip;}}
}

/**
Classe : SingleSoundOneShotClip
Dérivé de SoundOneShotClip, représente un son qui ne se joue qu'une fois depuis un seul clip
*/

[System.Serializable]
public class SingleSoundOneShotClip : SoundOneShotClip {
    [SerializeField]
    private AudioClip clip = null;
    public override AudioClip Clip {get{return clip;}}
}

/**
Classe : RandomSoundOneShotClip
Dérivé de SoundOneShotClip, représente un son qui ne se joue qu'une fois depuis plusieurs clip choisi au hasard
*/

[System.Serializable]
public class RandomSoundOneShotClip : SoundOneShotClip {
    [SerializeField]
    private List<AudioClip> clip = null;
    public override AudioClip Clip {
        get{
            int n = clip.Count;
            if(n == 0) return null;
            return clip[Random.Range(0, n)];
        }
    }
}
