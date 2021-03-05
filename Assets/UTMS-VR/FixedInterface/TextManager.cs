using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FixedInterface {
    public class TextManager : MonoBehaviour {
        public FixedInterfaceEvent firstEvents;
        public FixedInterfaceEvent events;
        Text text;
        // Start is called before the first frame update
        void Start() {
            this.text = this.gameObject.GetComponent<Text>();
            SetupEvents();
            var setting = CreateSetting(true);
            firstEvents.Invoke(setting);
            ApplySetting(setting);
        }

        // Update is called once per frame
        void Update() {
            var setting = CreateSetting(false);
            events.Invoke(setting);
            ApplySetting(setting);
        }

        private void SetupEvents() {
            if (this.events == null) {
                this.events = new FixedInterfaceEvent();
            }
        }

        private FixedInterfaceSetting CreateSetting(bool first) {
            var setting = new FixedInterfaceSetting();
            if (!first) {
                setting.text = this.text.text;
                setting.font = this.text.font;
                setting.fontSize = this.text.fontSize;
                setting.color = this.text.color;
            }
            return setting;
        }

        private void ApplySetting(FixedInterfaceSetting setting) {
            this.text.text = setting.text;
            this.text.font = setting.font;
            this.text.fontSize = setting.fontSize;
            this.text.color = setting.color;
        }

    }
}
