using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace DungeonShop.GameLogic
{
    public class DelaySceneLoader : MonoBehaviour
    {
#if UNITY_EDITOR
        public Object scene;
#endif

#pragma warning disable 0649
        public float delay;
        [HideInInspector]
        [SerializeField]
        private string sceneName = "";
        [SerializeField]
        private bool loadOnEnable = false;

        [Space(10f)]
        public UnityEvent OnSceneLoadingStarted;
#pragma warning restore 0649

        private static List<DelaySceneLoader> allLoaders = new List<DelaySceneLoader>();

#if UNITY_EDITOR
        public void OnValidate()
        {
            sceneName = "";

            if (scene != null)
            {
                if (scene.ToString().Contains("(UnityEngine.SceneAsset)"))
                    sceneName = scene.name;
                else
                    scene = null;
            }
        }
#endif

        private void OnEnable()
        {
            if (loadOnEnable)
                Load(delay);
        }

        public void Load()
        {
            StartCoroutine(LoadSceneCor(sceneName, delay));
        }

        public void Load(float delay)
        {
            StartCoroutine(LoadSceneCor(sceneName, delay));
        }

        private IEnumerator LoadSceneCor(string name, float delay)
        {
            if (delay != 0f)
                yield return new WaitForSeconds(delay);

            if (OnSceneLoadingStarted != null) OnSceneLoadingStarted.Invoke();

            SceneManager.LoadScene(name);

            yield return null;
        }

        public void SetScene(string sceneName)
        {
            this.sceneName = sceneName;
        }

        public void SetScene(Object sceneObject)
        {
            if (sceneObject == null)
                return;

            if (sceneObject.ToString().Contains("(UnityEngine.SceneAsset)"))
            {
                sceneName = sceneObject.name;

#if UNITY_EDITOR
                scene = sceneObject;
#endif
            }
        }

        public string GetSceneName()
        {
            return sceneName;
        }
    }
}