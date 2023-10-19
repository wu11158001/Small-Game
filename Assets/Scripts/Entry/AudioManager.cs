using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StaticScripts;

/// <summary>
/// 遊戲啟動入口
/// </summary>
namespace Entry
{
    /// <summary>
    /// 音效管理
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        static AudioManager audioManager;
        public static AudioManager Instance => audioManager;

        ObjectPool objectPool;//物件池

        [Tooltip("音樂本地紀錄名稱")] readonly string[] musicName = { "Music", "Sound" };
        [Tooltip("音效集")] AudioClip[] soundClips;

        [Tooltip("音樂控制器")] [SerializeField] AudioSource musicSource;
        [Tooltip("音效物件")] [SerializeField] GameObject soundObj;
        [Tooltip("物件池音效物件編號")] int audioPoolId;
        [Tooltip("音效資源")] AudioClip[] audioClips;

        [Tooltip("音量開關(0=音樂,1=音效)")] public bool[] musicSwitch;

        private void Awake()
        {
            if (audioManager != null)
            {
                Destroy(this);
                return;
            }
            audioManager = this;
            DontDestroyOnLoad(this);

            musicSwitch = new bool[2];//音量開關(0=音樂,1=音效)

            soundObj.SetActive(false);

            objectPool = ObjectPool.Instance;//物件池
        }

        private void Start()
        {
            //產生初始物件
            audioPoolId = objectPool.OnCreateAndRecordObject(soundObj, transform);
            //載入音效資源
            audioClips = Resources.LoadAll<AudioClip>("Audio");

            //音效集
            soundClips = new AudioClip[] 
            {
                serchSound("ButtonClick"),  //0 = 按鈕點擊音效
                serchSound("Score"),        //1 = 得分音效
                serchSound("Win"),          //2 = 獲勝音效
                serchSound("GameOver"),     //3 = 遊戲結束音效
            };

            //獲取本地音樂開關紀錄
            int musicValue = PlayerPrefs.GetInt(musicName[0]);//音樂開關值(0=關)
            int soundValue = PlayerPrefs.GetInt(musicName[1]);//音效開關值(0=關)
            musicSwitch[0] = musicValue == 0 ? false : true;
            musicSwitch[1] = soundValue == 0 ? false : true;
            musicSource.volume = musicSwitch[0] == true ? 0.3f : 0;//音樂音量
        }

        /// <summary>
        /// 播放按鈕點擊音效
        /// </summary>
        public void OnPlayButtonClickSound()
        {
            PlaySound(soundClips[0]);
        }

        /// <summary>
        /// 播放得分音效
        /// </summary>
        public void OnPlayScoreSound()
        {
            PlaySound(soundClips[1]);
        }

        /// <summary>
        /// 播放遊戲結束音效
        /// </summary>
        /// <param name="isWin">是否獲勝</param>
        public void OnPlayGameOverSound(bool isWin)
        {
            if (isWin) PlaySound(soundClips[2]);
            else PlaySound(soundClips[3]);
        }

        /// <summary>
        /// 音樂開關
        /// </summary>
        public void OnMusicSwitch()
        {
            musicSwitch[0] = !musicSwitch[0];////音量開關(0=音樂,1=音效)
            musicSource.volume = musicSwitch[0] == true ? 1 : 0;

            PlayerPrefs.SetInt(musicName[0], (int)musicSource.volume);
        }

        /// <summary>
        /// 音效開關
        /// </summary>
        public void OnSoundSwitch()
        {
            musicSwitch[1] = !musicSwitch[1];////音量開關(0=音樂,1=音效)
            int volume = musicSwitch[1] == true ? 1 : 0;
            PlayerPrefs.SetInt(musicName[1], volume);
        }

        /// <summary>
        /// 尋找音效
        /// </summary>
        /// <param name="clipName">音效名稱</param>
        /// <returns></returns>
        AudioClip serchSound(string clipName)
        {
            AudioClip clip = null;
            for (int i = 0; i < audioClips.Length; i++)
            {
                if (clipName == audioClips[i].name)
                {
                    clip = audioClips[i];
                    break;
                }
            }

            return clip;
        }

        /// <summary>
        /// 設定播放音效
        /// </summary>
        /// <param name="clipName">音效名稱</param>
        public void SetPlaySound(string clipName)
        {
            AudioClip clip = serchSound(clipName);

            if (clip == null)
            {
                Debug.LogError("播放音效不存在:" + clipName);
                return;
            }

            //播放音效
            PlaySound(clip);
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip">音效</param>
        void PlaySound(AudioClip clip)
        {
            //音效關閉
            if (!musicSwitch[1]) return;

            objectPool.OnActiveObject(audioPoolId, transform).TryGetComponent<AudioSource>(out AudioSource source);
            if (source != null)
            {
                source.clip = clip;
                source.Play();
                StartCoroutine(IEnableSoundObj(source.gameObject, source.clip.length));
            }
        }

        /// <summary>
        /// 關閉音效物件
        /// </summary>
        /// <param name="soundObj">音效物件</param>
        /// <param name="delayTime">音效播放長度</param>
        /// <returns></returns>
        IEnumerator IEnableSoundObj(GameObject soundObj, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            if (soundObj != null)
            {
                soundObj.SetActive(false);
            }
        }

        /// <summary>
        /// 物件池物件刪除_音訊
        /// </summary>
        public void OnCleanObjPoolForAudio()
        {
            objectPool.OnDestroyObject(audioPoolId);
        }
    }
}