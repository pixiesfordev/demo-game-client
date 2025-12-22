using UnityEngine;

namespace tower.Main {
    public class StartSceneManager : MonoBehaviour {
        void Start() {
            BaseManager.CreateNewInstance();
        }
    }
}