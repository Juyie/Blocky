using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

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
            line = Instantiate(linePref);

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
        // �ι�° ����� ������ ���
        if (rightTriggerReference.action.ReadValue<float>() > 0.0f && rightGripReference.action.ReadValue<float>() == 0.0f && startObject != null && blockObj != startObject)
        {
            endObject = blockObj;
            line.GetComponent<Line>().SetEndObject(endObject);

            // collider ��ġ�� ũ�⵵ �ٲپ��ش�.
            BoxCollider collider = line.GetComponent<BoxCollider>();
            float centerX = ((startObject.transform.position.x * endObject.transform.localScale.x + endObject.transform.position.x * startObject.transform.localScale.x) / (endObject.transform.localScale.x + startObject.transform.localScale.x)) / 2;
            float centerY = ((startObject.transform.position.y * endObject.transform.localScale.x + endObject.transform.position.y * startObject.transform.localScale.x) / (endObject.transform.localScale.x + startObject.transform.localScale.x)) / 2;
            float centerZ = ((startObject.transform.position.z * endObject.transform.localScale.x + endObject.transform.position.z * startObject.transform.localScale.x) / (endObject.transform.localScale.x + startObject.transform.localScale.x)) / 2;
            collider.center = new Vector3(centerX, centerY, centerZ);
            float distance = Mathf.Sqrt(Mathf.Pow(startObject.transform.position.x - endObject.transform.position.x, 2) +
                Mathf.Pow(startObject.transform.position.y - endObject.transform.position.y, 2) +
                Mathf.Pow(startObject.transform.position.z - endObject.transform.position.z, 2));
            float lenX = Mathf.Abs(startObject.transform.position.x - endObject.transform.position.x) / Mathf.Sqrt(Mathf.Pow(startObject.transform.position.x - endObject.transform.position.x, 2));
            float lenY = Mathf.Abs(startObject.transform.position.y - endObject.transform.position.y) / Mathf.Sqrt(Mathf.Pow(startObject.transform.position.y - endObject.transform.position.y, 2));
            float lenZ = Mathf.Abs(startObject.transform.position.z - endObject.transform.position.z) / Mathf.Sqrt(Mathf.Pow(startObject.transform.position.z - endObject.transform.position.z, 2));
            collider.size = new Vector3(lenX, lenY, lenZ);

            line = null;
            startObject = null;
            endObject = null;
        }
            // ���� ���������� �ۼ����ش�.
        if (leftTriggerReference.action.ReadValue<float>() > 0.0f && leftGripReference.action.ReadValue<float>() == 0.0f && startObject == null)
        {
            startObject = blockObj;
            endObject = GameObject.Find("LeftFront");

            line = Instantiate(linePref);

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
        if (leftTriggerReference.action.ReadValue<float>() > 0.0f && leftGripReference.action.ReadValue<float>() == 0.0f && startObject != null && blockObj != startObject)
        {
            endObject = blockObj;
            line.GetComponent<Line>().SetEndObject(endObject);
            line = null;
            startObject = null;
            endObject = null;
        }
    }

    private void Update()
    {
        //Debug.Log(count);
    }
}
