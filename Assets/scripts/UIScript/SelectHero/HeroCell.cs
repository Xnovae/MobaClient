using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib {
    public class HeroCell : IUserInterface {
        public int hid = 0;
        private void Awake()
        {
            SetCallback("Button", OnSelect);
        }
        public void SetData(int id, string job)
        {
            GetLabel("ID").text = id.ToString();
            GetLabel("Hero").text = job;
            hid = id;
            if(SelectHero.Instance.selectCell == this)
            {
                GetName("Button").SetActive(false);
            }else
            {
                GetName("Button").SetActive(true);
            }
        }

        private void OnSelect()
        {
            SelectHero.Instance.selectCell = this;
            SelectHero.Instance.UpdateUI();
        }
    }

}