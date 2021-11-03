using UnityEngine;
using DungeonShop.GameLogic;
using DungeonShop.Miscellaneous;

namespace DungeonShop.Visuals
{
    public class StyleSwitcher : MonoBehaviour, IVisualStyleSwitcher
    {
#pragma warning disable 0649
        [SerializeField]
        private Material regularMaterial;
        [SerializeField]
        private Material simpleMaterial;
#pragma warning restore 0649

        GameConfiguration gameConfiguration;
        private Renderer[] renderers;
        private bool awaken = false;
        private bool started = false;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            awaken = true;
        }

        private void Start()
        {
            if (!awaken)
                Awake();

            gameConfiguration = ConfigurationObject.GameConfiguration;
            if (gameConfiguration != null)
                ChangeStyle(gameConfiguration.style);

            started = true;
        }

        private void Update()
        {
            if (!started)
                Start();
        }

        public void ChangeStyle(Styles style)
        {
            if (!awaken)
                Awake();

            System.Action action;
            switch (style)
            {
                case Styles.Regular:
                    action = () => ReplaceeOneMaterialWithAnother(simpleMaterial, regularMaterial);
                    break;
                case Styles.Simple:
                    action = () => ReplaceeOneMaterialWithAnother(regularMaterial, simpleMaterial);
                    break;
                default:
                    action = () => { };
                    break;
            }

            StartCoroutine(Extensions.SkipFrames(1, action));
        }

        private void ReplaceeOneMaterialWithAnother(Material mat1, Material mat2)
        {
            if (renderers == null)
                return;

            for (int i = 0; i < renderers.Length; i++)
            {
                for (int m = 0; m < renderers[i].sharedMaterials.Length; m++)
                {
                    Material[] mats = renderers[i].sharedMaterials;
                    mats[m] = mats[m].Equals(mat1) ? mat2 : mats[m];
                    renderers[i].sharedMaterials = mats;
                }
            }
        }
    }
}
