using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoundSystem;

public class ActorSoundController : SoundController
{
    private SoundManager soundManager;
    private List<SoundSource> walkSounds;
    private List<SoundSource> runSounds;
    private Coroutine _footstepSoundCoroutine;
    private int footstepSoundCount;
    private bool _isRun;

    private void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        soundManager = SoundManager.Instance;
        InitSoundLists();
    }

    private void InitSoundLists()
    {
        walkSounds = soundManager.soundSources.GetSoundSourcesByNameContain("Walk");
        runSounds = soundManager.soundSources.GetSoundSourcesByNameContain("Run");
        footstepSoundCount = Mathf.Min(walkSounds.Count, walkSounds.Count);
    }

    IEnumerator FootSoundPlay()
    {
        int footstepSoundIndex = 0;

        while (true)
        {
            audioSource.clip = (_isRun ? runSounds : walkSounds)[footstepSoundIndex].sound;
            audioSource.Play();

            yield return new WaitWhile(() => audioSource.isPlaying);

            footstepSoundIndex = (footstepSoundIndex + 1) % footstepSoundCount;
        }
    }

    public void StartFootstepSoundPlay(bool isRun)
    {
        _isRun = isRun;
        if (_footstepSoundCoroutine == null)
        {
            _footstepSoundCoroutine = StartCoroutine(FootSoundPlay());
        }
    }

    public void StopFootstepSoundPlay()
    {
        if (_footstepSoundCoroutine != null)
        {
            StopCoroutine(_footstepSoundCoroutine);
            _footstepSoundCoroutine = null;
        }
    }
}
