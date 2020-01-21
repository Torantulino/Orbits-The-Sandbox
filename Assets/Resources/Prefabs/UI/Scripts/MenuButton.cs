using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Resources.Prefabs.UI.Scripts
{
    public class MenuButton : MonoBehaviour
    {
        [SerializeField] MenuButtonController menuButtonController;
        [SerializeField] Animator animator;
        [SerializeField] AnimatorFunctions animatorFunctions;
        [SerializeField] int thisIndex;

		void Start()
		{
			animatorFunctions.disableOnce = true;
		}

        // Update is called once per frame
        void Update()
        {
            if (menuButtonController.index == thisIndex)
            {
                animator.SetBool("selected", true);
                if (Input.GetAxis("Submit") == 1)
                {
                    animator.SetBool("pressed", true);
                }
                else if (animator.GetBool("pressed"))
                {
                    animator.SetBool("pressed", false);
                    animatorFunctions.disableOnce = true;
                }
            }
            else
            {
                animator.SetBool("selected", false);
            }
        }
    
		void StartGame()
		{
			SceneManager.LoadScene("Realism");
		}
	}
}