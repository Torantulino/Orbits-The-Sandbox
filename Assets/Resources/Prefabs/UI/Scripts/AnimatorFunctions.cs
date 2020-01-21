using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Resources.Prefabs.UI.Scripts
{
    public class AnimatorFunctions : MonoBehaviour
    {
        [SerializeField] MenuButtonController menuButtonController;
        public bool disableOnce;

        void PlaySound(AudioClip whichSound)
        {
			
            if (!disableOnce)
            {
                menuButtonController.audioSource.PlayOneShot(whichSound);
            }
            else
            {
                disableOnce = false;
            }
        }
    }
}