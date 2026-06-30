using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 10;
    [SerializeField] private bool expandPoolIfNeeded = true;

    [Header("3D Sound Settings")]
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

    private readonly List<AudioSource> audioPool = new List<AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;


        InitializePool();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void InitializePool()
    {
        audioPool.RemoveAll(source => source == null);

        while (audioPool.Count < poolSize)
        {
            CreateAudioSource();
        }
    }

    private AudioSource CreateAudioSource()
    {
        GameObject obj = new GameObject($"PooledSound_{audioPool.Count}");
        obj.transform.SetParent(transform);

        AudioSource source = obj.AddComponent<AudioSource>();
        ApplyAudioSourceSettings(source);

        audioPool.Add(source);

        return source;
    }

    private void ApplyAudioSourceSettings(AudioSource source)
    {
        source.playOnAwake = false;
        source.spatialBlend = spatialBlend;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = rolloffMode;
    }

    private AudioSource GetAvailableSource()
    {
        // Destroy済みのAudioSourceをリストから削除
        audioPool.RemoveAll(source => source == null);

        foreach (AudioSource source in audioPool)
        {
            if (source == null)
            {
                continue;
            }

            if (!source.isPlaying)
            {
                return source;
            }
        }

        // 全部使用中なら、必要に応じて追加生成
        if (expandPoolIfNeeded)
        {
            return CreateAudioSource();
        }

        return null;
    }

    public void PlaySound(
        AudioClip clip,
        Vector3 position,
        float volume,
        float minPitch = 0.9f,
        float maxPitch = 1.1f
    )
    {
        if (clip == null)
        {
            return;
        }

        AudioSource source = GetAvailableSource();

        if (source == null)
        {
            Debug.LogWarning("使用可能なAudioSourceがありません。");
            return;
        }

        Vector3 soundPosition = position;

        if (Camera.main != null)
        {
            soundPosition.z = Camera.main.transform.position.z;
        }

        source.transform.position = soundPosition;
        source.clip = clip;
        source.volume = volume;
        source.pitch = Random.Range(minPitch, maxPitch);

        source.Play();
    }
}