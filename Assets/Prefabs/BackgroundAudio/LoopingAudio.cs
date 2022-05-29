using UnityEngine;
using System.Collections;

public class LoopingAudio : MonoBehaviour
{
    // Unity configurable fields
    public AudioClip intro;
    public AudioClip loop;
    public float fadeDuration = 2F;
    // Whether or not this replaces any existing global background audio. Set to false for temporary audio overrides
    public bool globalBackground = false;

    // Reference to the current background audio
    private static LoopingAudio currentBackgroundAudio;
    // Cloned child object references
    private Animation introAnimation;
    private AudioSource introPlayer;

    // Used to ensure we don't stop an audio that was changed by another component when performing a delayed stopped
    // If this is changed, it means we started playing an audio again 
    private int playCount = 0;

    // Start playing the currently configured audio clips. You may call this function directly after dynamically setting the audio clip
    void OnEnable() {
        playCount++;
        if (!intro && !loop && !globalBackground) {
            // Nothing to do - or no audio yet configured
            // The global background check is important - some scenes may whish to just clear the background audio
            return;
        }
        // This component will clone itself to have another audio source with the same settings
        // We prefer this method to avoid configuration duplication on the audio sources
        // That means the LoopingAudio script will also be duplicated. We have to implement a bit of logic
        // to make sure the child copy does not try to handle the audio on its own
        #nullable enable
        Transform? parent = gameObject.transform.parent;
        // Do we have a parent?
        if (parent != null) {
            LoopingAudio? parentLoopingAudio = parent.GetComponent<LoopingAudio>();
            // Is the parent component holding a LoopingAudio? If so, we're in the child copy. Don't do anything
            if (parentLoopingAudio != null) {
                return; // Don't interfere with the parent
            }
        }
        #nullable disable

        // Is this audio expected to replace the previous background audio?
        if (globalBackground) {
            // Is there already an existing current background audio?
            if (currentBackgroundAudio) {
                // Is the current background audio playing the same audio loop? If so, we don't want to start it again, keep it playing
                if (currentBackgroundAudio.loop == loop) {
                    return;
                }
                // Else, ask it to fade out
                currentBackgroundAudio.Destroy();
            }
            // We are the new global background audio. Store us
            currentBackgroundAudio = this;
        }

        // Actual audio handling: schedule the audio clips so that they are perfectly stitched
        AudioSource loopPlayer = GetComponent<AudioSource>();
        // Give the audio player a bit of time before playing the audio (so it can load the file)
        double start = AudioSettings.dspTime + 0.05;
        if (intro) {
            // Duplicate the whole game object (to have the fadeout animation on the intro as well)
            // Duplication also allows us to avoid having 2 identical audio sources to configure
            if (introPlayer == null) {
                introPlayer = AudioSource.Instantiate(loopPlayer, gameObject.transform);
            }
            introPlayer.loop = false;
            introPlayer.clip = intro;
            introPlayer.volume = 1f;
            // Fetch the animation of the duplicated object, we'll need it when fading out
            introAnimation = introPlayer.GetComponent<Animation>();
            // Schedule audio assets so that they are perfectly stitched 
            introPlayer.PlayScheduled(start);
            // The next asset shall start at the end of the intro
            start += intro.length;
        }
        if (loop) {
            loopPlayer.loop = true;
            loopPlayer.clip = loop;
            loopPlayer.volume = 1f;
            loopPlayer.PlayScheduled(start);
        }
        // Make sure our objects are not removed when loading another scene so we have time to fade out
        if (globalBackground) {
            DontDestroyOnLoad(gameObject);
        }
    }

    // Fade in the audio loop
    void FadeIn(bool stopAfterFade = true, float? durationOverride = null) {
        playCount++;
        // Run the fade out animation, in reverse
        FadeOut(stopAfterFade, -1f * (durationOverride ?? fadeDuration));
    }

    // Fade out the audio loop
    void FadeOut(bool stopAfterFade = true, float? durationOverride = null) {
        var animation = GetComponent<Animation>();
        float duration = durationOverride ?? fadeDuration;
        if (duration != 0 && animation.clip.length > 0) {
            // Scale the animation to make sure it lasts the expected duration
            float speed = 1f / (duration * animation.clip.length);
            PlayAnimationAtSpeed(animation, speed);
            if (introAnimation) {
                PlayAnimationAtSpeed(introAnimation, speed);
            }
        }
        if (stopAfterFade) {
            // Give us one more half second to ensure the animations have completed
            StartCoroutine(DelayedStop(duration + .5f, playCount));
        }
    }

    IEnumerator DelayedStop(float delay, int expectedPlayCount) {
        // Wait for the animation to complete
        yield return new WaitForSeconds(delay);
        // See if the audio is being played again or was changed in the meantime. If so, don't stop it
        if (playCount != expectedPlayCount) {
            yield break;
        }
        if (introPlayer) {
            introPlayer.Stop();
        }
        GetComponent<AudioSource>().Stop();
    }

    void Destroy() {
        // Run the Fade out effect
        FadeOut(stopAfterFade: false);
        // Clean up our data after the fade out completed
        GameObject.Destroy(gameObject, fadeDuration);
    }

    void PlayAnimationAtSpeed(Animation animation, float speed) {
        foreach (AnimationState state in animation) {
            state.speed = speed;
        }
        animation.Play();
    }
}
