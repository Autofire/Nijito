using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace Dialogue.VN {
	public class BackgroundController : MonoBehaviour {
		[Tooltip("This is the main background layer that's normally used.")]
		[SerializeField] private Image currentBG;
		[Tooltip("This one is used for fading out to a new BG. Should render on top of the current.")]
		[SerializeField] private Image overlayBG;
		[SerializeField] private string bgSpritePath = "Backgrounds";


		[Header("Animations")]
		[SerializeField] private Animator anim;
		[SerializeField] private AnimationSettings animSettings;
		//[SerializeField] private string idleAnimName = "None";
		[SerializeField] private string transitionAnimName = "FadeToNew";

		private void Awake() {
			Assert.IsNotNull(currentBG);
			Assert.IsNotNull(overlayBG);
			Assert.IsNotNull(anim);
		}

		public void Switch(string bgName, Speed speed, bool wait, System.Action onComplete) {
			// Sprites are bundled per-image, so we have to load the whole thing and then
			// extract the background image.
			Sprite[] spriteSheet = Resources.LoadAll<Sprite>(bgSpritePath + "/" + bgName);

			if(spriteSheet.Length == 0) {
				Debug.LogWarning("Could not find background " + bgName);
				onComplete?.Invoke();
			}
			else {
				overlayBG.sprite = currentBG.sprite;
				currentBG.sprite = spriteSheet[0];

				StartCoroutine(animSettings.PlayAnim(anim, transitionAnimName, speed, wait, onComplete));
			}

		}
	}
}
