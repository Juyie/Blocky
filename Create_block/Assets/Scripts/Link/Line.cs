using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class Line : MonoBehaviour, IPunObservable
{
    // ��Ʈ�ѷ� input���� �޾ƿ��� ���� ���
    [SerializeField]
    private InputActionReference rightTriggerReference;

    [SerializeField]
    private InputActionReference leftTriggerReference;

    // LineManager���� �Ѱ��ִ� ���� �����ϱ� ���� ����
    public LineManager lineManager;
    private GameObject startObject;
    private GameObject endObject;

    // ��ϵ��� ��ġ�� ���ߴ��� �˱� ���� ����
    private Vector3 startOriginPosition;
    private Vector3 endOriginPosition;

    // ���� ������ ���� ����
    private PhotonView photonView;
    private int[] tempObjectIDs;
    private bool isMine = false;

    // startObject�� endObject�� �����ϱ� ���� LineManager���� ȣ���ؼ� ����ϴ� �Լ�
    public void SetStartObject(GameObject go)
    {
        startObject = go;
        startOriginPosition = go.transform.position;
        isMine = true;
    }

    public void SetEndObject(GameObject go)
    {
        endObject = go;
        endOriginPosition = go.transform.position;
    }

    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isMine)
        {
            if (startObject == null || endObject == null)
            {
                Debug.Log("Destroy Line");
                Destroy(gameObject);
            }
            else
            {
                // ���� ����� ��ġ�� ���ߴٸ� ���� ��ġ�� ���߾� ���� ��ġ�� �ٲپ��ش�.
                // ��ϵ��� originPosition�� ������Ʈ ���ش�.
                if (startOriginPosition != startObject.transform.position || endOriginPosition != endObject.transform.position)
                {
                    gameObject.GetComponent<LineRenderer>().SetPosition(0, startObject.transform.position);
                    gameObject.GetComponent<LineRenderer>().SetPosition(1, endObject.transform.position);
                    startOriginPosition = startObject.transform.position;
                    endOriginPosition = endObject.transform.position;

                    // collider ��ġ�� ũ�⵵ �ٲپ��ش�.
                    BoxCollider collider = gameObject.GetComponent<BoxCollider>();
                    // ���� ���� ���
                    float startVectorX = endObject.transform.position.x - startObject.transform.position.x;
                    float startVectorY = endObject.transform.position.y - startObject.transform.position.y;
                    float startVectorZ = endObject.transform.position.z - startObject.transform.position.z;
                    Vector3 startNormal = new Vector3(startVectorX, startVectorY, startVectorZ).normalized;
                    Vector3 endNormal = new Vector3(-startNormal.x, -startNormal.y, -startNormal.z);
                    // �뷫���� ǥ�� ��ǥ ���
                    Vector3 startSurface = startObject.transform.position + startNormal * startObject.transform.localScale.x;
                    Vector3 endSurface = endObject.transform.position + endNormal * endObject.transform.localScale.x;
                    // ���� ��ǥ ���
                    Vector3 colliderCenter = (startSurface + endSurface) / 2;
                    collider.center = colliderCenter;
                    // ũ�� ����
                    float lenX = Mathf.Abs(startObject.transform.position.x - endObject.transform.position.x) / 10;
                    float lenY = Mathf.Abs(startObject.transform.position.y - endObject.transform.position.y) / 10;
                    float lenZ = Mathf.Abs(startObject.transform.position.z - endObject.transform.position.z) / 2;
                    collider.size = new Vector3(lenX, lenY, lenZ);
                }

                // ���� endObject�� ��Ʈ�ѷ��� ��쿡 ���� �����̴�.
                // ���� ��Ʈ�ѷ��� trigger�� ������ �ִٸ� ��Ʈ�ѷ��� ��ġ�� ���� ���� ��ġ�� ������Ʈ ���ش�.
                if (rightTriggerReference.action.ReadValue<float>() > 0.0f || leftTriggerReference.action.ReadValue<float>() > 0.0f)
                {
                    gameObject.GetComponent<LineRenderer>().SetPosition(0, startObject.transform.position);
                    gameObject.GetComponent<LineRenderer>().SetPosition(1, endObject.transform.position);
                }
                // trigger�� ���� �� ���� endObject�� ��Ʈ�ѷ���� line�� destroy�Ѵ�. �׷��� ������ ���� �����ȴ�.
                // �׸��� ���� ���� �׸� �� �ֵ��� LineManager�� num���� �ʱ�ȭ���ش�.
                else
                {
                    if (endObject.name == "LeftFront" || endObject.name == "RightFront")
                    {
                        lineManager.ResetObject();
                        Destroy(gameObject);
                    }
                }
            }
        }
        else
        {
            gameObject.GetComponent<LineRenderer>().SetPosition(0, startObject.transform.position);
            gameObject.GetComponent<LineRenderer>().SetPosition(1, endObject.transform.position);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            tempObjectIDs = new int[] {startObject.GetComponent<PhotonView>().ViewID, endObject.GetComponent<PhotonView>().ViewID };
            stream.SendNext(tempObjectIDs);
        }
        else
        {
            tempObjectIDs = (int[])stream.ReceiveNext();
            startObject = PhotonView.Find(tempObjectIDs[0]).gameObject;
            endObject = PhotonView.Find(tempObjectIDs[1]).gameObject;
        }
    }
}
