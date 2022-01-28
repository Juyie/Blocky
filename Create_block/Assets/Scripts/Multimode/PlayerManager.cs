using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerManager : MonoBehaviour, IPunObservable
{

    #region Public Fields
    public Transform head;
    public Transform left;
    public Transform right;
    #endregion

    #region Private Fields
    private PhotonView photonView;
    private Transform headRig;
    private Transform leftRig;
    private Transform rightRig;
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        XRRig rig = FindObjectOfType<XRRig>();
        headRig = rig.transform.Find("Camera Offset/Main Camera");
        leftRig = rig.transform.Find("Camera Offset/LeftHand Controller");
        rightRig = rig.transform.Find("Camera Offset/RightHand Controller");

        //�ִϸ��̼� ���� ��� �ؿ� setActive �κ� �����
        /*
         * if(PhotonView.isMine)
         * {
         *  foreach (var item in GetComponentInChildren<Renderer>())
         *      {   
         *            item.enabled = false;
         *      }
         * }
         */

        /*
         * ���� �����ϴ� ��� ��ϵ�� ���ε��� ������
         */
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            head.gameObject.SetActive(false);
            left.gameObject.SetActive(false);
            right.gameObject.SetActive(false);

            mapPosition(head, headRig);
            mapPosition(left, leftRig);
            mapPosition(right, rightRig);
        }
        else
        {
            /*
             * ��ϵ�� ���ε��� ���� ������(�ؿ��� ������ ������)
             */
        }
    }

    void mapPosition(Transform target, Transform rigTransform)
    {
        target.position = rigTransform.position;
        target.rotation = rigTransform.rotation;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            /*
             * �����ϴ� ��ϵ�� ���ε��� ������ ������
             */
        }
        else
        {
            /*
             * ������ �޾Ƽ� ������
             */
        }
    }
}
