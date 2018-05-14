using UnityEngine;
using System.Collections;

namespace MyLib
{
    public class BackgroundSound : MonoBehaviour
    {
        public static BackgroundSound Instance;
        AudioSource source;
        GameObject player;
        private AudioSource effectSource;
        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            source = GetComponent<AudioSource>();
            effectSource = gameObject.AddComponent<AudioSource>();
            effectSource.volume = 1;
            //gameObject.GetComponent<AudioListener>().enabled = false;
        }

        public void PlaySound(string sound, float volume=1)
        {
            var clip = Resources.Load<AudioClip>("sound/" + sound);
            source.clip = clip;
            source.volume = volume;
            source.Play();
        }

        public void PlayEffectPos(string sound , Vector3 pos){
            var clip = Resources.Load<AudioClip>("sound/" + sound);
            AudioSource.PlayClipAtPoint(clip, pos);
        }

        public void PlayEffect(string sound, bool lowerBg = false){
            Log.Sys("PlayerEffect:"+sound);
            var clip = Resources.Load<AudioClip>("sound/" + sound);
            if(clip == null){
                clip = Resources.Load<AudioClip>("sound/skill/" + sound);
            }
            if (lowerBg)
            {
                source.volume = 0.1f;
                StartCoroutine(WaitReset(clip.length));
            }
            effectSource.PlayOneShot(clip);
        }

        public void PlayEffect(string sound, float volumn)
        {
            Log.Sys("PlayerEffect:"+sound);
            var clip = Resources.Load<AudioClip>("sound/" + sound);
            if(clip == null){
                clip = Resources.Load<AudioClip>("sound/skill/" + sound);
            }

            effectSource.PlayOneShot(clip, volumn);
        }

        IEnumerator WaitReset(float t)
        {
            yield return new WaitForSeconds(Mathf.Max(2f, t));
            source.volume = 1;
        }

        /// <summary>
        /// 需要手动 Player 这个AudioSource
        /// </summary>
        /// <param name="sound"></param>
        /// <returns></returns>
        public AudioSource  PlayEffectLoop(string sound, float volume=1) {
            Log.Sys("PlaySound: "+sound);
            var clip = Resources.Load<AudioClip>("sound/" + sound);
            var audio = gameObject.AddComponent<AudioSource>();
            audio.loop = true;
            audio.volume = volume;
            audio.clip = clip;
            return audio;
        }

        void Update()
        {
            if (player == null)
            {
                player = ObjectManager.objectManager.GetMyPlayer();
            }
            if (player != null)
            {
                transform.position = player.transform.position;
            }
        }
    }
}
