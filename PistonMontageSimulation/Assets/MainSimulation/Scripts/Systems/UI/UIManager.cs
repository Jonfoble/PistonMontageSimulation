using PistonProject.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
	public TextMeshProUGUI assemblyCompleteText; 
	public GameObject restartButton;

	private void OnEnable()
	{
		AssemblyManager.Instance.onAssemblyComplete.AddListener(OnAssemblyComplete);
		assemblyCompleteText.gameObject.SetActive(false); // Hide text initially
		if (restartButton != null)
			restartButton.SetActive(false);
	}

	private void OnDisable()
	{
		AssemblyManager.Instance.onAssemblyComplete.RemoveListener(OnAssemblyComplete);
	}

	private void OnAssemblyComplete()
	{
		assemblyCompleteText.gameObject.SetActive(true);
		if (restartButton != null)
			restartButton.SetActive(true); // Show the pause panel
	}

	public void ResetGame()
	{
		assemblyCompleteText.gameObject.SetActive(false);
		if (restartButton != null)
			restartButton.SetActive(false); // Hide the pause panel

		// Reset the game
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
