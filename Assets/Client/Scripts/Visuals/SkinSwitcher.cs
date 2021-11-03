using DungeonShop.GameLogic;
using DungeonShop.Miscellaneous;
using UnityEngine;

namespace DungeonShop.Visuals
{
    public class SkinSwitcher : MonoBehaviour, IVisualStyleSwitcher
    {
#pragma warning disable 0649
        [SerializeField]
        private Transform simpleSkinRoot;
        [SerializeField]
        private Transform regularSkinRoot;
#pragma warning disable 0649

        private Renderer[] regularRenderrers = new Renderer[] { };
        private Renderer[] simpleRenderrers = new Renderer[] { };

        GameConfiguration gameConfiguration;
        private Styles style;
        bool awaken = false;
        bool started = false;

        private void Awake()
        {
            if (regularSkinRoot != null)
                regularRenderrers = regularSkinRoot.GetComponentsInChildren<Renderer>();

            if (simpleSkinRoot != null)
                simpleRenderrers = simpleSkinRoot.GetComponentsInChildren<Renderer>();

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
            this.style = style;

            // Because otherwise unity gives wierd warning
            StartCoroutine(Extensions.SkipFrames(1, SetSkin));
        }

        private void SetSkin()
        {
            for (int i = 0; i < simpleRenderrers.Length; i++)
            {
                simpleRenderrers[i].enabled = style == Styles.Simple;
            }

            for (int i = 0; i < regularRenderrers.Length; i++)
            {
                regularRenderrers[i].enabled = style == Styles.Regular;
            }
        }
    }
}
