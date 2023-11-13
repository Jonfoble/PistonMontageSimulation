using PistonProject.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class UIManager : MonoBehaviour
{
	public TextMeshProUGUI assemblyCompleteText; // Assign this in the inspector
	public GameObject restartButton; // Assign a panel to show when the game is paused

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
		// Show the assembly complete message and pause the game
		assemblyCompleteText.gameObject.SetActive(true);
		if (restartButton != null)
			restartButton.SetActive(true); // Show the pause panel
	}

	// Call this method when the reset button is pressed
	public void ResetGame()
	{
		// Hide the assembly complete message and resume the game
		assemblyCompleteText.gameObject.SetActive(false);
		if (restartButton != null)
			restartButton.SetActive(false); // Hide the pause panel

		// Reset the game state or reload the scene
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
