using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SoundSystem
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