using tower.Main;
using UnityEngine;

namespace tower.Battle {
    public class BattleSceneManager : MonoBehaviour {
        void Start() {
            BaseManager.CreateNewInstance();
        }
    }
}