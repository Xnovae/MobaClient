using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyLib
{
    public class SelectHero : IUserInterface
    {
        public static SelectHero Instance;

        UIGrid grid;
        GameObject cell;
        List<GameObject> cells = new List<GameObject>();
        public HeroCell selectCell;
        private void Awake()
        {
            Instance = this;

            grid = GetGrid("Grid");
            cell = GetName("Cell");
            cell.SetActive(false);
            SetCallback("StartGame", OnStart);
            UpdateUI();
        }
        void OnStart()
        {
            if(selectCell != null)
            {
                NetMatchScene.Instance.ChooseHero(selectCell.hid);
            }
        }

        public override void UpdateUI()
        {
            SetData();
        }
        void SetData()
        {
            var allJobs = GameData.RoleJobDescriptions;
            while(cells.Count < allJobs.Count)
            {
                var c = GameObject.Instantiate<GameObject>(cell);
                c.transform.parent = grid.transform;
                Util.InitGameObject(c);
                cells.Add(c);
            }

            foreach(var c in cells)
            {
                c.SetActive(false);
            }

            var i = 0;
            foreach(var j in allJobs)
            {
                var id = j.id;
                var n = j.job;
                var c = cells[i];
                var hc = c.GetComponent<HeroCell>();
                hc.SetData(id, n);
                c.SetActive(true);
                i++;
            }
            grid.repositionNow = true;
        }
    }
}
