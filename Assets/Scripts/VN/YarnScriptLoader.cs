using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

namespace Dialogue.VN {
	public class YarnScriptLoader : MonoBehaviour {
		//public YarnProgram defaultScript;
		public UnityEvent onMissingScript;
		[SerializeField] private DialogueRunner runner;

		private IEnumerator Start() {

			yield return null; // HACK -- Makes sure the scene is fully loaded

			YarnProgram script = Settings.selectedYarn; //?? defaultScript;

			if (script == null) {
				onMissingScript.Invoke();
			}
			else {
				runner.Add(script);
				runner.StartDialogue(script.GetProgram().Nodes.First().Key);
			}
		}

	}
}
