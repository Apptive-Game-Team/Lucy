using System;
using System.Collections.Generic;
using UnityEngine;

namespace soundSystem_
{
    [Serializable]
    public struct SoundSource
    {
        public int id;
        public string name;
        public SoundType type;
        public AudioClip sound;
    }

    public enum SoundType
    {
        BACKGROUND,      // 배경 음악
        UNIT,            // 유닛의 소리 (캐릭터나 몬스터의 소리)
        INTERACTION,     // UI 상호작용 소리
        EFFECT,          // 효과음 (폭발, 스킬 등)
        VOICE,           // 캐릭터 음성
        AMBIENT          // 환경 소리 (바람, 물 흐르는 소리 등)
    }

    [CreateAssetMenu]
    public class SoundSources : ScriptableObject
    {

        [SerializeField]
        List<SoundSource> soundSources = new List<SoundSource>();

        public SoundSource? GetByName(string name)
        {
            foreach (SoundSource source in soundSources)
            {
                if (source.name.Equals(name))
                {
                    return source;
                }
            }
            return null;
        }

        /// <summary>
        /// Clip for a sound name, or null with a warning when it is missing.
        /// Callers used to do GetByName(name).Value, which throws on an unknown name.
        /// </summary>
        public AudioClip GetClipByName(string name)
        {
            SoundSource? source = GetByName(name);
            if (source == null)
            {
                Debug.LogWarning($"Sound '{name}' not found in SoundSources");
                return null;
            }
            return source.Value.sound;
        }

        public SoundSource? GetById(int id)
        {
            foreach (SoundSource source in soundSources)
            {
                if (source.id == id)
                {
                    return source;
                }
            }
            return null;
        }

        public List<SoundSource> GetSoundSourcesByNameContain(string subName)
        {
            List<SoundSource> targets = new List<SoundSource>();

            foreach (SoundSource source in soundSources)
            {
                if (source.name.Contains(subName))
                {
                    targets.Add(source);
                }
            }

            return targets;
        }
    }
}