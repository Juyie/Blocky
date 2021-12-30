using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class JoinManager : MonoBehaviour
{
    public bool isJoined = false;

    private int count = 0;
    private GameObject objects0;
    private GameObject objects1;

    [SerializeField]
    private InputActionReference rightGripReference;
    [SerializeField]
    private InputActionReference leftGripReference;

    [SerializeField]
    private Create create;


    public void SetObject(GameObject obj)
    {
        if (count == 0)
        {
            if (obj != objects1) // ���� ����� 2�� ��ϵ��� �ʵ���
                objects0 = obj;
        }
        else
        {
            if(obj != objects0)
                objects1 = obj;
        }
            
        count = (count + 1) % 2;

        if (objects0 != null && objects1 != null)
            Debug.Log(objects0.name + ", " + objects1.name);
    }

    void Update()
    {
        // ���� ��Ʈ�ѷ��� ��� �ִ°� �ƴ� ��쿡�� �������� �ʵ��� �ٽ� null���� �ִ´�.
        if (rightGripReference.action.ReadValue<float>() <= 0.0f || leftGripReference.action.ReadValue<float>() <= 0.0f)
        {
            objects0 = null;
            objects1 = null;
        }
        else
        {
            if (objects0 != null && objects1 != null)
            {
                Debug.Log("Join!");
                Vector3 pos = objects0.transform.position;
                Vector3 scale = objects0.transform.localScale;
                string msgText = objects0.transform.GetChild(2).GetComponent<TextMesh>().text;
                string otherMsg = objects1.transform.GetChild(2).GetComponent<TextMesh>().text;

                Destroy(objects0);
                Destroy(objects1);

                GameObject newBlock = (GameObject)Instantiate(Resources.Load("Prefab/Block"));
                newBlock.name = create.GetI().ToString();
                create.IncreaseI();
                newBlock.transform.localScale = scale * 2;
                newBlock.transform.position = pos;
                Vector3 attachPoint = newBlock.transform.GetChild(0).localPosition;
                newBlock.transform.GetChild(0).localPosition = new Vector3(attachPoint.x, attachPoint.y, attachPoint.z / 2);
                newBlock.GetComponent<MeshRenderer>().material.color = Color.green;
                newBlock.GetComponent<Rigidbody>().useGravity = false;
                newBlock.GetComponent<Rigidbody>().isKinematic = true;
                newBlock.GetComponent<XRGrabInteractable>().enabled = true;
                newBlock.GetComponentInChildren<XRSimpleInteractable>().enabled = true;
                newBlock.GetComponent<FlyBlock>().SetIsDone();
                newBlock.transform.GetChild(2).GetComponent<TextMesh>().text = msgText + "+" + otherMsg;
                newBlock.transform.tag = "Old";

                objects0 = null;
                objects1 = null;
            }
        }
    }
}