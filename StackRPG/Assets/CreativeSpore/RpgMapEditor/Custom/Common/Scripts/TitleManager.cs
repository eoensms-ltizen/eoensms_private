using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{	
	void Start () {
        SceneManager.LoadScene("MainMenu");
	}
}
