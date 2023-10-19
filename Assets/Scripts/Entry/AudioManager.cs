using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StaticScripts;

/// <summary>
/// �C���ҰʤJ�f
/// </summary>
namespace Entry
{
    /// <summary>
    /// ���ĺ޲z
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        static AudioManager audioManager;
        public static AudioManager Instance => audioManager;

        ObjectPool objectPool;//�����

        [Tooltip("���֥��a�����W��")] readonly string[] musicName = { "Music", "Sound" };
        [Tooltip("���Ķ�")] AudioClip[] soundClips;

        [Tooltip("���ֱ��")] [SerializeField] AudioSource musicSource;
        [Tooltip("���Ī���")] [SerializeField] GameObject soundObj;
        [Tooltip("��������Ī���s��")] int audioPoolId;
        [Tooltip("���ĸ귽")] AudioClip[] audioClips;

        [Tooltip("���q�}��(0=����,1=����)")] public bool[] musicSwitch;

        private void Awake()
        {
            if (audioManager != null)
            {
                Destroy(this);
                return;
            }
            audioManager = this;
            DontDestroyOnLoad(this);

            musicSwitch = new bool[2];//���q�}��(0=����,1=����)

            soundObj.SetActive(false);

            objectPool = ObjectPool.Instance;//�����
        }

        private void Start()
        {
            //���ͪ�l����
            audioPoolId = objectPool.OnCreateAndRecordObject(soundObj, transform);
            //���J���ĸ귽
            audioClips = Resources.LoadAll<AudioClip>("Audio");

            //���Ķ�
            soundClips = new AudioClip[] 
            {
                serchSound("ButtonClick"),  //0 = ���s�I������
                serchSound("Score"),        //1 = �o������
                serchSound("Win"),          //2 = ��ӭ���
                serchSound("GameOver"),     //3 = �C����������
            };

            //������a���ֶ}������
            int musicValue = PlayerPrefs.GetInt(musicName[0]);//���ֶ}����(0=��)
            int soundValue = PlayerPrefs.GetInt(musicName[1]);//���Ķ}����(0=��)
            musicSwitch[0] = musicValue == 0 ? false : true;
            musicSwitch[1] = soundValue == 0 ? false : true;
            musicSource.volume = musicSwitch[0] == true ? 0.3f : 0;//���֭��q
        }

        /// <summary>
        /// ������s�I������
        /// </summary>
        public void OnPlayButtonClickSound()
        {
            PlaySound(soundClips[0]);
        }

        /// <summary>
        /// ����o������
        /// </summary>
        public void OnPlayScoreSound()
        {
            PlaySound(soundClips[1]);
        }

        /// <summary>
        /// ����C����������
        /// </summary>
        /// <param name="isWin">�O�_���</param>
        public void OnPlayGameOverSound(bool isWin)
        {
            if (isWin) PlaySound(soundClips[2]);
            else PlaySound(soundClips[3]);
        }

        /// <summary>
        /// ���ֶ}��
        /// </summary>
        public void OnMusicSwitch()
        {
            musicSwitch[0] = !musicSwitch[0];////���q�}��(0=����,1=����)
            musicSource.volume = musicSwitch[0] == true ? 1 : 0;

            PlayerPrefs.SetInt(musicName[0], (int)musicSource.volume);
        }

        /// <summary>
        /// ���Ķ}��
        /// </summary>
        public void OnSoundSwitch()
        {
            musicSwitch[1] = !musicSwitch[1];////���q�}��(0=����,1=����)
            int volume = musicSwitch[1] == true ? 1 : 0;
            PlayerPrefs.SetInt(musicName[1], volume);
        }

        /// <summary>
        /// �M�䭵��
        /// </summary>
        /// <param name="clipName">���ĦW��</param>
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
        /// �]�w���񭵮�
        /// </summary>
        /// <param name="clipName">���ĦW��</param>
        public void SetPlaySound(string clipName)
        {
            AudioClip clip = serchSound(clipName);

            if (clip == null)
            {
                Debug.LogError("���񭵮Ĥ��s�b:" + clipName);
                return;
            }

            //���񭵮�
            PlaySound(clip);
        }

        /// <summary>
        /// ���񭵮�
        /// </summary>
        /// <param name="clip">����</param>
        void PlaySound(AudioClip clip)
        {
            //��������
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
        /// �������Ī���
        /// </summary>
        /// <param name="soundObj">���Ī���</param>
        /// <param name="delayTime">���ļ������</param>
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
        /// ���������R��_���T
        /// </summary>
        public void OnCleanObjPoolForAudio()
        {
            objectPool.OnDestroyObject(audioPoolId);
        }
    }
}