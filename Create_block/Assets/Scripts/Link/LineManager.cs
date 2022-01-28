using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;

public class LineManager : MonoBehaviour
{
    // line�� prefab�� clone���� ����� ������� ����Ѵ�.
    [SerializeField]
    private GameObject linePref;

    // line�� material
    [SerializeField]
    private Material blackMtl;

    // ��Ʈ�ѷ� input���� �޾ƿ��� ���� ���
    [SerializeField]
    private InputActionReference rightTriggerReference;

    [SerializeField]
    private InputActionReference leftTriggerReference;

    [SerializeField]
    private InputActionReference rightGripReference;

    [SerializeField]
    private InputActionReference leftGripReference;

    // start object�� end object�� �����ϱ� ���� ����
    private GameObject startObject = null;
    private GameObject endObject = null;

    // ������ line�� �����ϱ� ���� ����
    private GameObject line;

    // ���� �����ǰų� �����Ǵ� ��� object�� �����ϱ� ���� ����ϴ� �Լ�
    public void ResetObject()
    {
        startObject = null;
        endObject = null;
    }

    // ���� ����� ���� ����ϴ� �Լ�
    public void CreateLine(GameObject blockObj)
    {
        // �׸��� ������ �ʰ� Ʈ���Ÿ� ������ �� �۵��ϵ��� ������ ���δ�.
        // �׸��� ù��° ����� ������ ��쿡�� ���� ���鵵�� ������ ���δ�.
        if (rightTriggerReference.action.ReadValue<float>() > 0.0f && rightGripReference.action.ReadValue<float>() == 0.0f && startObject == null)
        {
            // ù��° ��ϰ� ��Ʈ�ѷ��� ����Ǿ�� �ϹǷ� endObject���� ��Ʈ�ѷ� object�� ã�� �־��ش�.
            startObject = blockObj;
            endObject = GameObject.Find("RightFront");

            // prefab�� ����Ͽ� ������ ������ش�.
            line = PhotonNetwork.Instantiate(this.linePref.name, new Vector3(0, 0, 0), Quaternion.identity);

            // line�� �Ӽ��� �����ϱ� ���� component�� LineRenderer�� lr�� �����Ѵ�.
            // �׸��� ���� material, �β� ���� �����ش�.
            LineRenderer lr = line.GetComponent<LineRenderer>();

            lr.startColor = Color.black;
            lr.endColor = Color.black;
            lr.material = blackMtl;
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;

            // ���� ����Ǿ� �ִ� startObject�� endObject���� position�� �޾ƿ� �����Ѵ�.
            // �׸��� lr�� position�� �̸� �־��ش�.
            Vector3 startPos = startObject.transform.position;
            Vector3 endPos = endObject.transform.position;
            lr.SetPosition(0, startPos);
            lr.SetPosition(1, endPos);

            // line�� �����ϱ� ���� component�� Line�� ��������
            // �Լ��� ȣ���Ͽ� startObject, endObject, LineManager�� �Ѱ��ش�.
            line.GetComponent<Line>().SetStartObject(startObject);
            line.GetComponent<Line>().SetEndObject(endObject);
            line.GetComponent<Line>().lineManager = gameObject.GetComponent<LineManager>();
        }
        // ���� ���������� �ۼ����ش�.
        if (leftTriggerReference.action.ReadValue<float>() > 0.0f && leftGripReference.action.ReadValue<float>() == 0.0f && startObject == null)
        {
            startObject = blockObj;
            endObject = GameObject.Find("LeftFront");

            line = PhotonNetwork.Instantiate(this.linePref.name, new Vector3(0,0,0), Quaternion.identity);

            LineRenderer lr = line.GetComponent<LineRenderer>();
            lr.startColor = Color.black;
            lr.endColor = Color.black;
            lr.material = blackMtl;
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;

            Vector3 startPos = startObject.transform.position;
            Vector3 endPos = endObject.transform.position;
            lr.SetPosition(0, startPos);
            lr.SetPosition(1, endPos);

            line.GetComponent<Line>().SetStartObject(startObject);
            line.GetComponent<Line>().SetEndObject(endObject);
            line.GetComponent<Line>().lineManager = gameObject.GetComponent<LineManager>();
        }
        // �ι�° ����� ������ ���
        if ((rightTriggerReference.action.ReadValue<float>() > 0.0f && rightGripReference.action.ReadValue<float>() == 0.0f
            || leftTriggerReference.action.ReadValue<float>() > 0.0f && leftGripReference.action.ReadValue<float>() == 0.0f) 
            && startObject != null && blockObj != startObject)
        {
            endObject = blockObj;
            line.GetComponent<Line>().SetEndObject(endObject);

            // collider ��ġ�� ũ�⵵ �ٲپ��ش�.
            BoxCollider collider = line.GetComponent<BoxCollider>();
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

            line = null;
            startObject = null;
            endObject = null;
        }
    }
}
