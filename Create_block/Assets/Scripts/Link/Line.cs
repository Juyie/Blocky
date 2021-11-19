using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Line : MonoBehaviour
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

    // startObject�� endObject�� �����ϱ� ���� LineManager���� ȣ���ؼ� ����ϴ� �Լ�
    public void SetStartObject(GameObject go)
    {
        startObject = go;
        startOriginPosition = go.transform.position;
    }

    public void SetEndObject(GameObject go)
    {
        endObject = go;
        endOriginPosition = go.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // ���� ����� ��ġ�� ���ߴٸ� ���� ��ġ�� ���߾� ���� ��ġ�� �ٲپ��ش�.
        // ��ϵ��� originPosition�� ������Ʈ ���ش�.
        if (startOriginPosition != startObject.transform.position || endOriginPosition != endObject.transform.position)
        {
            gameObject.GetComponent<LineRenderer>().SetPosition(0, startObject.transform.position);
            gameObject.GetComponent<LineRenderer>().SetPosition(1, endObject.transform.position);
            startOriginPosition = startObject.transform.position;
            endOriginPosition = endObject.transform.position;
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
                Destroy(gameObject);
            }
            lineManager.SetNumZero();
        }
    }
}
