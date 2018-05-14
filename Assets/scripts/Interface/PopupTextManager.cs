using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace MyLib {
	public class PopupTextManager : MonoBehaviour {
		static PopupTextManager _pop = null;
		public static PopupTextManager popTextManager{
			get {
				if(_pop == null) {
					var g = new GameObject("PopUptext");
					_pop = g.AddComponent<PopupTextManager>();
				}
				return _pop;
			}
		}
		public GameObject hudText = null;
		public GameObject popUpPanel = null;
		public GameObject purpleText = null;

	    private GameObject  GetHudLabel(GameObject parent, GameObject labelTemplate)
	    {
	        if (popUpPanel == null)
	        {
	            var temp = Resources.Load<GameObject>("UI/popUpDamage");
	            popUpPanel = NGUITools.AddChild(WindowMng.windowMng.GetUIRoot(), temp);
	        }
	        parent = popUpPanel;

	        if (labs.ContainsKey(labelTemplate.name) && labs[labelTemplate.name].Count > 0)
	        {
	            var lab = labs[labelTemplate.name].Dequeue();
                lab.SetActive(true);
	            return lab;
	        }
	        //var hudText = Resources.Load<GameObject>("UI/MyHudLabel");
            Log.Sys("HudText: "+labelTemplate);
	        var go = GameObject.Instantiate(labelTemplate) as GameObject;
	        go.name = labelTemplate.name;
	        go.transform.parent = parent.transform;
	        go.transform.localPosition = Vector3.zero;
	        go.transform.localRotation = Quaternion.identity;
	        go.transform.localScale = Vector3.one;
	        go.layer = parent.layer;
	        return go;
	    }

	    public void ShowText(object text, Transform target)
	    {
	        //var label = NGUITools.AddChild (popUpPanel, hudText);
	        var label = GetHudLabel(popUpPanel, hudText);
            var ft = label.GetComponent<FollowTarget>();
            ft.target = target.gameObject;
            ft.enabled = true;
	        //label.SetActive (true);

	        label.GetComponent<HUDText>().Add(text, Color.white, 2f);
	        StartCoroutine(WaitRemove(label));
	    }

	    private void Awake()
	    {
	        hudText = Resources.Load<GameObject>("UI/MyHudLabel");
	        purpleText = Resources.Load<GameObject>("UI/MyHudLabelPurple");
	    }

	    public void ShowWhiteText(object text, Transform target) {
			Log.GUI ("Show Red Text Here "+text);
	
			//var label = NGUITools.AddChild (popUpPanel, hudText);
		    var label = GetHudLabel(popUpPanel, hudText);
			label.GetComponent<FollowTarget> ().target = target.gameObject;
			//label.SetActive (true);
			
			label.GetComponent<HUDText> ().Add (text, new Color(1f, 1f, 1f), 0.1f);
			StartCoroutine (WaitRemove(label));
		}

        public void ShowYellowText(object text, Transform target)
        {
            Log.GUI("Show Red Text Here " + text);

            var label = GetHudLabel(popUpPanel, hudText);
            label.GetComponent<FollowTarget>().target = target.gameObject;
            //label.SetActive (true);

            label.GetComponent<HUDText>().Add(text, new Color(1f, 1f, 0f), 0.1f);
            StartCoroutine(WaitRemove(label));
        }

        public void ShowPurpleText (object text, Transform target)
		{
			//var label = NGUITools.AddChild (popUpPanel, purpleText);
            var label = GetHudLabel(popUpPanel, purpleText);
			label.GetComponent<FollowTarget> ().target = target.gameObject;
			//label.SetActive (true);
			
			label.GetComponent<HUDText> ().Add (text, new Color(223/255.0f, 31/255.0f, 246/255.0f), 0.1f);
			StartCoroutine (WaitRemove(label));
		}
        //private Queue<GameObject> labels = new Queue<GameObject>();
        private Dictionary<string, Queue<GameObject>> labs = new  Dictionary<string, Queue<GameObject>>();

	    private IEnumerator WaitRemove(GameObject label)
	    {
	        var text = label.GetComponent<HUDText>();
	        while (true)
	        {
	            if (!text.isVisible)
	            {
	                break;
	            }
	            yield return null;
	        }

            //GameObject.Destroy (label);
            //labels.Enqueue(label);

            yield return null;
	        if (!labs.ContainsKey(label.name))
	        {
	            labs.Add(label.name, new Queue<GameObject>());
	        }

            label.GetComponent<FollowTarget>().enabled = false;
            labs[label.name].Enqueue(label);
	        //label.SetActive(false);
	    }

	    void OnDestroy() {
			_pop = null;
		}
	}

}