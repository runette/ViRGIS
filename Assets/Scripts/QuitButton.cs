﻿using System.Threading.Tasks;
using UnityEngine;

namespace Virgis {
    public class QuitButton : MonoBehaviour {

        public async void OnClick() {
            Debug.Log("QuitButton.OnClick save before quit");
            if (AppState.instance.map != null) {
                MapInitialize mi = AppState.instance.map.GetComponentInChildren<MapInitialize>();
                await mi.Save(false);
            }
            Debug.Log("QuitButton.OnClick now quit");
            Application.Quit();
        }
    }
}
