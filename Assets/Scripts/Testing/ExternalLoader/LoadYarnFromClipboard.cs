using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Dialogue.Testing
{
	public class LoadYarnFromClipboard : MonoBehaviour
	{
		[SerializeField] private int testSceneIndex;
		[SerializeField] private Button buttonObj;

		private void OnEnable()
		{
			buttonObj.onClick.AddListener(Activate);
		}

		private void OnDisable()
		{
			buttonObj.onClick.RemoveListener(Activate);
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (hasFocus)
			{
				CheckClipboard();
			}
		}

		private void CheckClipboard()
		{
			bool valid = false;
			string clipboardContents = GUIUtility.systemCopyBuffer;

			if (!string.IsNullOrWhiteSpace(clipboardContents))
			{
				valid = true;
			}

			buttonObj.interactable = valid;
		}

		private void Activate() {

			string clipboardContents = GUIUtility.systemCopyBuffer;

			var program = YarnImporter.FromString("TEMP", clipboardContents);
			if (program == null)
			{
				Debug.LogError("Failed to load; not executing!");
				return;
			}
			
			Settings.selectedYarn = program;

			int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
			Settings.onFinish = () =>
			{
				Settings.selectedYarn = null;
				SceneManager.LoadScene(currentSceneIndex);
			};

			SceneManager.LoadScene(testSceneIndex);
		}
	}
}
