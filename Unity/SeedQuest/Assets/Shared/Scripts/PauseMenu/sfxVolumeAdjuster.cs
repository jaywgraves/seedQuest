﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sfxVolumeAdjuster : MonoBehaviour
{

    // This script should be attached to any object with an audiosource used for sound effect
    // This script will find the audiosource of the object, and adjust the volume.

    public GameStateData gameState;
    public AudioSource audioSource;

    public float baselineVol;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // If a base volume has been set for this source, set that volume
        if (baselineVol != 0)
        {
            audioSource.volume = baselineVol;
        }

    }

    void Update()
    {
        // only check for updates when game is paused

        if (gameState.isPaused)
        {

            if (gameState.musicMute)
            {
                //mute volume
                audioSource.volume = 0;
            }
            else
            {
                if (gameState.masterVolume == 0 || gameState.sfxVolume == 0)
                {
                    audioSource.volume = 0;
                }
                // Change volume based on baseline volume and optional volume settings
                else if (baselineVol != 0)
                {
                    audioSource.volume = gameState.masterVolume * gameState.sfxVolume * baselineVol;
                }
                // Change volume only using optional volume settings (no baseline volume set)
                else
                {
                    audioSource.volume = gameState.masterVolume * gameState.sfxVolume;
                }
            }
        }
    }
}

