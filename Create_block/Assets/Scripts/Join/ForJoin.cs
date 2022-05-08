using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class ForJoin : MonoBehaviour
{
    private JoinManager joinManager;

    void Start()
    {
        joinManager = GameObject.Find("JoinManager").GetComponent<JoinManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ???? ?????? ???? ???? ?????? ???????? ????
        if (gameObject.tag == "Old" && collision.transform.tag == "Old")
            joinManager.SetObject(gameObject);
    }
}