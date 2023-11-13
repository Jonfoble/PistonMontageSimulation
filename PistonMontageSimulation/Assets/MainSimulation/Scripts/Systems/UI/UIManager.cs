using PistonProject.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
	public TextMeshProUGUI assemblyCompleteText; 
	public GameObject restartButton;
	public GameObject exitButton;
	private void Awake()
	{
		// Hide text and buttons initially
		assemblyCompleteText.gameObject.SetActive(false); 

		if (restartButton != null)
			restartButton.SetActive(false);
		if (exitButton != null)
			exitButton.SetActive(false);
	}
	private void OnEnable()
	{
		AssemblyManager.Instance.onAssemblyComplete.AddListener(OnAssemblyComplete);
		
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

		if (exitButton != null)
			exitButton.SetActive(true);
	}

	public void ResetGame()
	{
		assemblyCompleteText.gameObject.SetActive(false);
		if (restartButton != null)
			restartButton.SetActive(false); // Hide the pause panel

		// Reset the game
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
	public void ExitGame()
	{
		Application.Quit();
	}
}
