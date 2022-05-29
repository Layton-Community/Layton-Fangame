using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    public void OnStartButtonPressed()
    {
        Debug.Log("Button clicked!");
        // TODO Put a spinner while we're loading the next frame with LoadSceneAsync instead.
       SceneManager.LoadScene("NameInput");
    }
}
