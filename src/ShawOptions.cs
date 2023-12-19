using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Silkslug
{
    public class ShawOptions : OptionInterface
    {
        public ShawOptions()
        {
            RewindKeys = new Configurable<KeyCode>[4];
            RewindKeys[0] = config.Bind("rewindKeyCode1", KeyCode.LeftControl);
            RewindKeys[1] = config.Bind("rewindKeyCode2", KeyCode.None);
            RewindKeys[2] = config.Bind("rewindKeyCode3", KeyCode.None);
            RewindKeys[3] = config.Bind("rewindKeyCode4", KeyCode.None);

            shawVoice = config.Bind("shawVoice", true);
        }

        public override void Initialize()
        {
            base.Initialize();


            ConsoleWrite("ShawOptions_Initialize");
            InGameTranslator inGameTranslator = Custom.rainWorld.inGameTranslator;

            Tabs = new OpTab[1];
            Tabs[0] = new OpTab(this, inGameTranslator.Translate("Options"));

            Tabs[0].AddItems(new OpLabel(new Vector2(20f, 520f), new Vector2(560f, 30f), "The Shaw", FLabelAlignment.Center, true, null));

            float width = 150f;
            float width2 = LabelTest.GetWidth(inGameTranslator.Translate("DISABLE SHAW VOICE"), false) + 15f;
            if (width2 > width)
            {
                width = width2;
            }

            disableShawVoice = new OpHoldButton(new Vector2(223f, 300f), Vector2.zero, "", 0f);
            disableShawVoice.OnPressDone += DisableShawVoicel_OnPressDone;
            disableShawVoice.description = " ";
            SetHoldButtonParameters();

            Tabs[0].AddItems(disableShawVoice);
        }

        public void SetHoldButtonParameters()
        {
            disableShawVoice.text = shawVoice.Value ? "DISABLE SHAW VOICE" : "ENABLE SHAW VOICE";
            disableShawVoice.colorEdge = shawVoice.Value ? Color.red : Color.white;
            disableShawVoice._fillTime = shawVoice.Value ? 900f : 80f;
            //float width = 150f;
            //float width2 = LabelTest.GetWidth(disableShawVoice.text, false) + 15f;
            //if (width2 > width)
            //{
            //    width = width2;
            //}
            //disableShawVoice.size = new Vector2(width, 35f);

        }

        private void DisableShawVoicel_OnPressDone(UIfocusable trigger)
        {
            shawVoice.Value = !shawVoice.Value;
            SetHoldButtonParameters();
        }

        public static ShawOptions instance = new();

        public static OpHoldButton disableShawVoice;

        public readonly Configurable<bool> shawVoice;

        public readonly Configurable<KeyCode>[] RewindKeys;
    }
}
