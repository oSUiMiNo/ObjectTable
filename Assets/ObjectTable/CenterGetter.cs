using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterGetter : MonoBehaviour
{
    public Transform frame;
    public Transform human;

    void Start()
    {


        //Center���W�����߂�
        Vector3 humanCenterPos = GetCenterPosition(human);

        //human���ǂꂾ���������ƁAPivot�̈ʒu��Center�������Ă����邩���߂�
        Vector3 centerDis = human.position - humanCenterPos;

        //frame�̍��W�ƁAhuman��Pivot��Center�̈ʒu�̍��𑫂��Ί���
        human.position = transform.position + centerDis;
    }


    public static Vector3 GetCenterPosition(Transform target)
    {
        //��A�N�e�B�u���܂߂āAtarget��target�̎q�S�Ẵ����_���[�ƃR���C�_�[���擾
        var cols = target.GetComponentsInChildren<Collider>(true);
        var rens = target.GetComponentsInChildren<Renderer>(true);

        //�R���C�_�[�ƃ����_���[���P���Ȃ���΁Atarget.position��center�ɂȂ�
        if (cols.Length == 0 && rens.Length == 0)
            return target.position;

        bool isInit = false;

        Vector3 minPos = Vector3.zero;
        Vector3 maxPos = Vector3.zero;

        for (int i = 0; i < cols.Length; i++)
        {
            var bounds = cols[i].bounds;
            var center = bounds.center;
            var size = bounds.size / 2;

            //�ŏ��̂P�x�����ʂ��āAminPos��maxPos������������
            if (!isInit)
            {
                minPos.x = center.x - size.x;
                minPos.y = center.y - size.y;
                minPos.z = center.z - size.z;
                maxPos.x = center.x + size.x;
                maxPos.y = center.y + size.y;
                maxPos.z = center.z + size.z;

                isInit = true;
                continue;
            }

            if (minPos.x > center.x - size.x) minPos.x = center.x - size.x;
            if (minPos.y > center.y - size.y) minPos.y = center.y - size.y;
            if (minPos.z > center.z - size.z) minPos.z = center.z - size.z;
            if (maxPos.x < center.x + size.x) maxPos.x = center.x + size.x;
            if (maxPos.y < center.y + size.y) maxPos.y = center.y + size.y;
            if (maxPos.z < center.z + size.z) maxPos.z = center.z + size.z;
        }
        for (int i = 0; i < rens.Length; i++)
        {
            var bounds = rens[i].bounds;
            var center = bounds.center;
            var size = bounds.size / 2;

            //�R���C�_�[���P���Ȃ���΂P�x�����ʂ��āAminPos��maxPos������������
            if (!isInit)
            {
                minPos.x = center.x - size.x;
                minPos.y = center.y - size.y;
                minPos.z = center.z - size.z;
                maxPos.x = center.x + size.x;
                maxPos.y = center.y + size.y;
                maxPos.z = center.z + size.z;

                isInit = true;
                continue;
            }

            if (minPos.x > center.x - size.x) minPos.x = center.x - size.x;
            if (minPos.y > center.y - size.y) minPos.y = center.y - size.y;
            if (minPos.z > center.z - size.z) minPos.z = center.z - size.z;
            if (maxPos.x < center.x + size.x) maxPos.x = center.x + size.x;
            if (maxPos.y < center.y + size.y) maxPos.y = center.y + size.y;
            if (maxPos.z < center.z + size.z) maxPos.z = center.z + size.z;
        }

        return (minPos + maxPos) / 2;
    }
}