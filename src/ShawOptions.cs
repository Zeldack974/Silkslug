using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
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


            Plugin.Log("ShawOptions_Initialize");
            InGameTranslator inGameTranslator = Custom.rainWorld.inGameTranslator;

            Tabs = new OpTab[1];
            Tabs[0] = new OpTab(this, inGameTranslator.Translate("Options"));

            Tabs[0].AddItems(new OpLabel(new Vector2(20f, 520f), new Vector2(560f, 30f), "The Shaw", FLabelAlignment.Center, true, null));

            //float width = 150f;
            //float width2 = LabelTest.GetWidth(inGameTranslator.Translate("DISABLE SHAW VOICE"), false) + 15f;
            //if (width2 > width)
            //{
            //    width = width2;
            //}

            disableShawVoiceCheckBox = new OpCheckBox(shawVoice, Vector2.zero);
            disableShawVoiceCheckBox.Hidden = true;
            disableShawVoiceCheckBox.OnValueChanged += DisableShawVoiceCheckBox_OnValueChanged;
            Tabs[0].AddItems(disableShawVoiceCheckBox);

            disableShawVoice = new OpHoldButton(new Vector2(223f, 300f), Vector2.zero, "", 0f);
            disableShawVoice.OnPressDone += DisableShawVoicel_OnPressDone;
            disableShawVoice.description = " ";

            Tabs[0].AddItems(disableShawVoice);
        }


        public void SetHoldButtonParameters()
        {
            disableShawVoice.text = disableShawVoiceCheckBox.GetValueBool() ? "DISABLE SHAW VOICE" : "ENABLE SHAW VOICE";
            disableShawVoice.colorEdge = disableShawVoiceCheckBox.GetValueBool() ? Color.red : Color.white;
            disableShawVoice._fillTime = disableShawVoiceCheckBox.GetValueBool() ? 900f : 80f;
            float width = 150f;
            float width2 = LabelTest.GetWidth(disableShawVoice.text, false) + 15f;
            if (width2 > width)
            {
                width = width2;
            }

            disableShawVoice.size = new Vector2(width, 35f);

        }

        private void DisableShawVoicel_OnPressDone(UIfocusable trigger)
        {
            disableShawVoiceCheckBox.SetValueBool(!disableShawVoiceCheckBox.GetValueBool());
        }

        private void DisableShawVoiceCheckBox_OnValueChanged(UIconfig config, string value, string oldValue)
        {
            SetHoldButtonParameters();
        }

        public static ShawOptions instance = new();

        public static OpHoldButton disableShawVoice;

        public static OpCheckBox disableShawVoiceCheckBox;

        public readonly Configurable<bool> shawVoice;

        public readonly Configurable<KeyCode>[] RewindKeys;
    }
}
