using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader 
{
    public enum Scene
    {
        MainScene,
        LobbyScene,
        LoadingScene
    }
    private static Scene targetScene;

    public static void Load(Scene scene){
        Loader.targetScene = scene;
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }
    public static void LoaderCallback() {
        SceneManager.LoadScene(targetScene.ToString());
    }

    public static void LoadNetworkManager(Scene scene) {
        Loader.targetScene = scene;
        //* PROXIMAMENTE AGREGAR LA LOAD SCENE EN NETWORK MANAGER
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }
}
