using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeVidSceneTransition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(GoToGameScene), 5);
    }

    private void GoToGameScene()
    {
        SceneManager.LoadScene(1);
    }
}
