using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class FlyBlock : MonoBehaviour
{
    // ��� ������ �������� Ȯ���Ѵ�.
    private bool isDone = false;

    public void SetIsDone()
    {
        isDone = true;
    }

    // action�� ������ �� �� �Լ��� isFly ������ �����Ѵ�.
    private bool isFly;

    public void SetIsFly(bool b)
    {
        isFly = b;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // ��ư�� ���� ������ �Ϸ�� �ĺ��� ������ �����ϴ�.
        if (isDone)
        {
            // isFly�� true�� �Ǹ� ����� ���߿� ����. 
            //�ݴ�� isFly�� false�� �Ǹ� �ٽ� �߷��� ������ �Ѵ�.
            if (isFly)
            {
                gameObject.GetComponent<Rigidbody>().useGravity = false;
                gameObject.GetComponent<Rigidbody>().isKinematic = true;
                gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            }
            else
            {
                gameObject.GetComponent<Rigidbody>().useGravity = true;
                gameObject.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }
}
