using Scoz.Func;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace tower.Main {
    public class MainSceneManager : MonoBehaviour {

        public static MainSceneManager Instance { get; private set; }

        private void Start() {
            Instance = this;
            BaseManager.CreateNewInstance();
        }

    }
}