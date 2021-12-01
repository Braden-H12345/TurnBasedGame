using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AudioClip _audio;
    private float _timeElapsed = 0f;
    private int count = 1;
    [SerializeField] float _volume = .75f;
    // Start is called before the first frame update
    void Start()
    {
        PlayBackground();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate()
    {
        _timeElapsed += Time.deltaTime;

        if (_timeElapsed >= _audio.length * count)
        {
            PlayBackground();
            count++;
        }
    }

    private void PlayBackground()
    {
        if (_audio != null)
        {
            AudioHelper.PlayClip2D(_audio, _volume);
        }
        
    }
}
