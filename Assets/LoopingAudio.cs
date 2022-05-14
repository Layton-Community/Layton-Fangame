using UnityEngine;

public class LoopingAudio : MonoBehaviour
{
    public AudioClip intro;
    public AudioClip loop;
    public float fadeDuration = 2F;

    private static LoopingAudio currentLoopingAudio;
    // Animation of the clone child (if any)
    private Animation introAnimation;


    void OnEnable()
    {
        if (currentLoopingAudio) {
            // Same audio loop? Keep it looping and don't do anything
            // This also ensures the cloned child (intro audio handling) won't interfere with the parent
            if (currentLoopingAudio.loop == loop) {
                return;
            }
            // Else, ask it to fade out
            currentLoopingAudio.Destroy();
        }
        // We are now the object handling background audio, store us in the static property
        currentLoopingAudio = this;
        AudioSource loopPlayer = GetComponent<AudioSource>();
        // Give the audio player a bit of time before playing the audio (so it can load the file)
        double start = AudioSettings.dspTime + 0.05;
        if (intro) {
            // Duplicate the whole game object (to have the fadeout animation on the intro as well)
            // Duplication also allows us to avoid having 2 identical audio sources to configure on the prefab
            AudioSource introPlayer = AudioSource.Instantiate(loopPlayer, gameObject.transform);
            introPlayer.loop = false;
            introPlayer.clip = intro;
            // Fetch the animation of the duplicated object, we'll need it when fading out
            introAnimation = introPlayer.GetComponent<Animation>();
            // Schedule audio assets so that they are perfectly stitched 
            introPlayer.PlayScheduled(start);
            start += intro.length;
        }
        if (loop) {
            loopPlayer.loop = true;
            loopPlayer.clip = loop;
            loopPlayer.PlayScheduled(start);
        }
        // Make sure our objects are not removed when loading another scene so we have time to fade out
        DontDestroyOnLoad(gameObject);
    }

    void Destroy()
    {
        // Fade out effect
        var animation = GetComponent<Animation>();
        if (fadeDuration > 0 && animation.clip.length > 0) {
            // Scale the animation to make sure it lasts the fade out duration
            var speed = 1 / (fadeDuration * animation.clip.length);
            PlayAnimationAtSpeed(animation, speed);
            if (introAnimation) {
                PlayAnimationAtSpeed(introAnimation, speed);
            }
        }
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
