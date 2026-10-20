using UnityEngine;
using UnityEngine.UI;

namespace CodeMonkey.Toolkit.TSimpleDestruction.Demo {

    /// <summary>
    /// ** Simple Destruction **
    /// 
    /// This is not exactly a tool but rather just a showcase
    /// of how easy it is to add this sort of logic to your game
    /// 
    /// Adding Destruction can make your game feel much more satisfying
    /// The trick is slicing your object into multiple pieces, spawning that prefab and applying
    /// an explosion force. 
    /// 
    /// Watch the Tutorial Video to learn more.
    /// </summary>
    public class Demo : MonoBehaviour {


        [SerializeField] private Transform cube;
        [SerializeField] private Transform cubePiecesPrefab;
        [SerializeField] private Button explodeButton;
        [SerializeField] private Button resetButton;


        private Transform cubePieces;


        private void Start() {
            explodeButton.onClick.AddListener(() => {
                cube.gameObject.SetActive(false);
                cubePieces = Instantiate(cubePiecesPrefab, cube.transform.position, cube.transform.rotation);
                explodeButton.gameObject.SetActive(false);
                resetButton.gameObject.SetActive(true);
            });

            resetButton.onClick.AddListener(() => {
                cube.gameObject.SetActive(true);
                Destroy(cubePieces.gameObject);
                explodeButton.gameObject.SetActive(true);
                resetButton.gameObject.SetActive(false);
            });
        }

    }

}