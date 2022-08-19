using System;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    public class Launch : MonoBehaviour
    {
        public EnumLanguage language;
        private void Awake()
        {
            Debug.Log(language);
           LocalizationMgr.Instance.InitLocalizatioContents(language);
        }

       
    }
}