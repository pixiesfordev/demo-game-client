using UnityEngine;
using tower.main;
using UnityEngine.SceneManagement;
using System;
using Cysharp.Threading.Tasks;

namespace Scoz.Func {
    public partial class TestTool : MonoBehaviour {

        [SerializeField] GameObject ToolGO;

        public static Animator MyAni;
        int key = 0;
        // Update is called once per frame
        void KeyDetector() {


            if (Input.GetKeyDown(KeyCode.Q)) {

            } else if (Input.GetKeyDown(KeyCode.W)) {

            } else if (Input.GetKeyDown(KeyCode.E)) {

            } else if (Input.GetKeyDown(KeyCode.R)) {

            } else if (Input.GetKeyDown(KeyCode.P)) {
            } else if (Input.GetKeyDown(KeyCode.O)) {

            } else if (Input.GetKeyDown(KeyCode.I)) {

            } else if (Input.GetKeyDown(KeyCode.L)) {
            }
        }


        public void OnModifyHP(int _value) {
        }
        public void OnModifySanP(int _value) {
        }
        public void ClearLocoData() {
            PlayerPrefs.DeleteAll();
        }

    }
}
