using UnityEngine;
using System;
using System.Linq;

namespace NeonShooter
{
    public static class Sound
    {
        public static AudioClip Music { get; private set; }

        private static readonly System.Random rand = new System.Random();

        private static AudioClip[] explosions;
        public static AudioClip Explosion { get { return explosions != null && explosions.Length > 0 ? explosions[rand.Next(explosions.Length)] : null; } }

        private static AudioClip[] shots;
        public static AudioClip Shot { get { return shots != null && shots.Length > 0 ? shots[rand.Next(shots.Length)] : null; } }

        private static AudioClip[] spawns;
        public static AudioClip Spawn { get { return spawns != null && spawns.Length > 0 ? spawns[rand.Next(spawns.Length)] : null; } }

        // 持久化的AudioSource,避免每次播放音效都创建临时GameObject
        private static AudioSource audioSource;

        public static void Load()
        {
            Music = Resources.Load<AudioClip>("Audio/Music");

            explosions = Enumerable.Range(1, 8).Select(x => Resources.Load<AudioClip>("Audio/explosion-0" + x)).Where(x => x != null).ToArray();
            shots = Enumerable.Range(1, 4).Select(x => Resources.Load<AudioClip>("Audio/shoot-0" + x)).Where(x => x != null).ToArray();
            spawns = Enumerable.Range(1, 8).Select(x => Resources.Load<AudioClip>("Audio/spawn-0" + x)).Where(x => x != null).ToArray();

            var go = new GameObject("SoundPlayer");
            audioSource = go.AddComponent<AudioSource>();
            UnityEngine.Object.DontDestroyOnLoad(go);
        }

        public static void PlayClip(AudioClip clip, float volume = 1f, float pitch = 1f, float pan = 0f)
        {
            if (clip == null || audioSource == null)
                return;

            audioSource.pitch = pitch;
            audioSource.panStereo = pan;
            audioSource.PlayOneShot(clip, volume);
        }
    }
}