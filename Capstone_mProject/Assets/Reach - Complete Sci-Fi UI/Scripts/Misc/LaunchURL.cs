﻿using UnityEngine;

namespace Michsky.UI.Reach
{
    public class LaunchURL : MonoBehaviour
    {
        public void GoToURL(string URL)
        {
            Application.OpenURL(URL);
        }
    }
}