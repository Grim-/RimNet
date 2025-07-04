using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimNet
{

    [StaticConstructorOnStartup]
    public class Mod_RimNet : Mod
    {
        private static Mod_RimNet instance;
        private Font font;
        private Font fontSmall;
        private Font fontTiny;
        private bool fontsLoaded = false;

        public Dictionary<GameFont, Font> Fonts = new Dictionary<GameFont, Font>();

        public Mod_RimNet(ModContentPack content) : base(content)
        {
            instance = this;
            // Don't load fonts in constructor
        }

        public void LoadFonts()
        {
            if (fontsLoaded) return;

            var bundle = this.Content.assetBundles.loadedAssetBundles?.FirstOrDefault(x => x.name == "rimnet");
            if (bundle == null) return; 
            font = bundle.LoadAsset<Font>("Font_Nasalisation");
            fontSmall = bundle.LoadAsset<Font>("Font_NasalisationSmall");
            fontTiny = bundle.LoadAsset<Font>("Font_NasalisationTiny");

            Fonts[GameFont.Medium] = font;
            Fonts[GameFont.Small] = fontSmall;
            Fonts[GameFont.Tiny] = fontTiny;

            fontsLoaded = true;
        }
    }
}