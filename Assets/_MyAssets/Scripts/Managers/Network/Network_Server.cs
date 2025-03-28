using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class Network_Server : MonoBehaviour
{    
    private void Start()
    {
        NetworkManager.Singleton.StartServer();
        //NetworkManager.Singleton.SceneManager.LoadScene("OpenWorld", LoadSceneMode.Single);
        NetworkManager.Singleton.SceneManager.LoadScene("Village_Network", LoadSceneMode.Additive);
        //StartCoroutine(OffsetSceneObjects("Village_Network", new Vector3(0, 3000, 0)));
    }
    

    IEnumerator OffsetSceneObjects(string sceneName, Vector3 offset)
    {
        yield return new WaitForSeconds(4);

        Debug.Log("Offsetting scene village");
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            Debug.Log("Village is loaded");
            foreach (GameObject obj in scene.GetRootGameObjects())
            {
                obj.transform.position += offset; // Move entire scene
            }
        }
    }

}