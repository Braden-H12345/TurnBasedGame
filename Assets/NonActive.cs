using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonActive : MonoBehaviour
{
    private void Awake()
    {
        this.gameObject.SetActive(false);
    }
}
