using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using Yarn.Compiler;
using UnityEngine.SceneManagement;
using System.IO;
using System.Globalization;

namespace Dialogue.Testing {

	public class LoadYarnFromPath : MonoBehaviour {
		[SerializeField] private InputField pathSrc;
		[SerializeField] private int testSceneIndex;

		private void Awake() {
			Assert.IsNotNull(pathSrc);
		}

		public void Activate() {

			if (string.IsNullOrEmpty(pathSrc.text))
			{
				Debug.LogError("Nothing to load!");
				return;
			}

			var program = YarnImporter.FromFile(pathSrc.text);

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
