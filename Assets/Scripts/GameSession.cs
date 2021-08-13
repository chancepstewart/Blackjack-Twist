using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Functions can be called in order to change the scene.
/// </summary>

public class GameSession : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void ChangeScene(int index)
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(index);
	}

	public void StartGame()
	{
		ChangeScene(1);
	}

	public void TitleScreen()
	{
		ChangeScene(0);
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void InstructionScreen()
	{
		ChangeScene(2);
	}
}
