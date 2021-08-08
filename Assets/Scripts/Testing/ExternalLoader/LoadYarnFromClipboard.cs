using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Dialogue.Testing
{
	public class LoadYarnFromClipboard : MonoBehaviour
	{
		[SerializeField] private int testSceneIndex;
		[SerializeField] private Button buttonObj;

		private string clipboardTitle;
		private string clipboardContents;

		private void OnEnable()
		{
			//CheckClipboard();
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
				ProcessClipboard();
			}
		}

		private void ProcessClipboard()
		{
			bool valid = false;
			clipboardTitle = "Untitled";
			clipboardContents = "";
			string rawBuffer = GUIUtility.systemCopyBuffer?.Trim();

			if (!string.IsNullOrWhiteSpace(rawBuffer))
			{
				//clipboardContents = Regex.Replace(clipboardContents, "^.*?title:", "title:", RegexOptions.Singleline);

				foreach (Match match in Regex.Matches(rawBuffer, @"^title: .*$", RegexOptions.Multiline))
				{
					//Debug.LogFormat("Found '{0}' at position {1}.", match.Value, match.Index);
					clipboardTitle = Regex.Replace(match.Value, @"title:\s*", "");
					Debug.Log(clipboardTitle);
				}

				string prefix = string.Format("title: {0}\n---\n", clipboardTitle);
				string body = Regex.Replace(rawBuffer, "^.*?<<", "<<", RegexOptions.Singleline);
				string suffix = rawBuffer.EndsWith("===") ? "" : "\n\n===";

				clipboardContents = prefix + body + suffix;
				Debug.Log(clipboardContents);

				valid = true;
			}

			buttonObj.interactable = valid;
		}

		private void Activate() {

			string clipboardContents = this.clipboardContents; //GUIUtility.systemCopyBuffer;

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
