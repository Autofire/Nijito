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

		void Start() {
			//ScriptSelector.FromName("TestScript"); // TODO Remove

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
