using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlasticGui.LaunchDiffParameters;


// �q�I�u�W�F�N�g��W�{������
public class Specimen : MonoBehaviour
{
    /// <summary>
    /// �q�I�u�W�F�N�g�̓���Bounds
    /// </summary>
    Bounds childObjBounds;

    Transform layout => GameObject.Find("ObjectTable").transform.Find("Grid Layout");
    Transform backGround => transform.Find("BackGround");

    Transform body;

    [SerializeField] bool createCollider;
    [SerializeField] public float size = 1f;
    [SerializeField] public Vector3 size3 = new Vector3(1, 1, 0.5f);


    void Start()
    {
        childObjBounds = CalcLocalObjBounds(gameObject);

        body = new GameObject().transform;
        body.parent = transform;
        body.localPosition = Vector3.zero;
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.name == "BackGround") continue;
            children.Add(child);
            child.transform.parent = body.transform;
        }

        // �W�{�ɂ��钆�g��������ΏI���
        if (children.Count == 0) return;

        // �L���[�u�̃T�C�Y�ƃ|�W�V�������o�E���f�B���O�{�b�N�X�ɍ��킹��
        // �q�I�u�W�F�N�g�ɉe�����Ȃ��悤�Ɉ�u�e�q�֌W����
        body.parent = null;
        transform.localScale = childObjBounds.size + childObjBounds.size / 10;
        transform.position = childObjBounds.center;
        body.parent = transform;

        //UnityEditor.EditorApplication.isPaused = true;

        float ratio = size / Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        transform.localScale *= ratio;

        body.parent = null;
        gameObject.transform.localScale = Vector3.one * size;
        body.parent = transform;

        transform.parent = layout.transform;

        // �s�{�b�g�����b�V���̒��S�ɖ����Q�[���I�u�W�F�N�g�ł��W�{�̐^�񒆂ɔz�u�����悤�ɂ���
        // Center���W�����߂�
        Vector3 humanCenterPos = GetCenterPosition(body);
        // body���ǂꂾ���������ƁAPivot�̈ʒu��Center�������Ă����邩���߂�
        Vector3 centerDis = body.position - humanCenterPos;
        // �W�{�Ɏq�̍��W�ƁAbody��Pivot��Center�̈ʒu�̍��𑫂��Ί���
        body.position = transform.position + centerDis;


        // Bounds�̑傫���ƌ`�󂪌����ڂɕ�����悤�R���C�_�[��ǉ�����
        if (createCollider)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        }
    }


    /// <summary>
    /// ���݃I�u�W�F�N�g�̃��[�J�����W�ł̃o�E���h�v�Z
    /// </summary>
    private Bounds CalcLocalObjBounds(GameObject obj)
    {
        // �w��I�u�W�F�N�g�̃��[���h�o�E���h���v�Z����
        Bounds totalBounds = CalcChildObjWorldBounds(obj, new Bounds());

        // ���[�J���I�u�W�F�N�g�̑��΍��W�ɍ��킹�ăo�E���h���Čv�Z����
        // �I�u�W�F�N�g�̃��[���h���W�ƃT�C�Y���擾����
        Vector3 ObjWorldPosition = transform.position;
        Vector3 ObjWorldScale = transform.lossyScale;

        // �o�E���h�̃��[�J�����W�ƃT�C�Y���擾����
        Vector3 totalBoundsLocalCenter = new Vector3(
            //(totalBounds.center.x - ObjWorldPosition.x) / ObjWorldScale.x,
            //(totalBounds.center.y - ObjWorldPosition.y) / ObjWorldScale.y,
            //(totalBounds.center.z - ObjWorldPosition.z) / ObjWorldScale.z);
            totalBounds.center.x,
            totalBounds.center.y,
            totalBounds.center.z);
        Vector3 meshBoundsLocalSize = new Vector3(
            totalBounds.size.x / ObjWorldScale.x,
            totalBounds.size.y / ObjWorldScale.y,
            totalBounds.size.z / ObjWorldScale.z);

        Bounds localBounds = new Bounds(totalBoundsLocalCenter, meshBoundsLocalSize);

        return localBounds;
    }


    /// <summary>
    /// �q�I�u�W�F�N�g�̃��[���h���W�ł̃o�E���h�v�Z�i�ċA�����j
    /// </summary>
    private Bounds CalcChildObjWorldBounds(GameObject obj, Bounds bounds)
    {
        // �w��I�u�W�F�N�g�̑S�Ă̎q�I�u�W�F�N�g���`�F�b�N����
        foreach (Transform child in obj.transform)
        {
            if (child.name == "BackGround") continue;

            // ���b�V���t�B���^�̑��݊m�F
            MeshFilter filter = child.gameObject.GetComponent<MeshFilter>();

            // �X�L���h���b�V�������_���̑��݊m�F
            SkinnedMeshRenderer skinnedRenderer = child.gameObject.GetComponent<SkinnedMeshRenderer>();

            Bounds meshBounds = new Bounds();

            if (filter != null)
            {
                // �t�B���^�[�̃��b�V����񂩂�o�E���h�{�b�N�X���擾����
                meshBounds = filter.mesh.bounds;
            }
            else
            if (skinnedRenderer != null)
            {
                // �X�L���h���b�V�������_������o�E���h�{�b�N�X���擾����
                meshBounds = skinnedRenderer.sharedMesh.bounds;
            }
            else
            {
                // �ċA����
                bounds = CalcChildObjWorldBounds(child.gameObject, bounds);
                continue;
            }

            // �I�u�W�F�N�g�̃��[���h���W�ƃT�C�Y���擾����
            Vector3 ObjWorldPosition = child.position;
            Vector3 ObjWorldScale = child.lossyScale;

            // �o�E���h�̃��[���h���W�ƃT�C�Y���擾����
            Vector3 meshBoundsWorldCenter = meshBounds.center + ObjWorldPosition;
            Vector3 meshBoundsWorldSize = Vector3.Scale(meshBounds.size, ObjWorldScale);

            // �o�E���h�̍ŏ����W�ƍő���W���擾����
            Vector3 meshBoundsWorldMin = meshBoundsWorldCenter - (meshBoundsWorldSize / 2);
            Vector3 meshBoundsWorldMax = meshBoundsWorldCenter + (meshBoundsWorldSize / 2);

            // �擾�����ŏ����W�ƍő���W���܂ނ悤�Ɋg��/�k�����s��
            if (bounds.size == Vector3.zero)
            {
                // ���o�E���h�̃T�C�Y���[���̏ꍇ�̓o�E���h����蒼��
                bounds = new Bounds(meshBoundsWorldCenter, Vector3.zero);
            }
            bounds.Encapsulate(meshBoundsWorldMin);
            bounds.Encapsulate(meshBoundsWorldMax);

            Debug.Log($"{child} {child.transform.localRotation.eulerAngles}");

            // �Q�[���I�u�W�F�N�g�̌��������ۂ̃��b�V���̌�������90�x�P�ʂŉ�]���Ă����ꍇ�A
            // ���ۂ̃��b�V���̃o�E���f�B���O�{�b�N�X���g���ƁA�V�[���ɔz�u���ꂽ�Q�[���I�u�W�F�N�g�ƌ���������Ă��܂�
            // �o�E���f�B���O�{�b�N�X�͉�]�ł��Ȃ����ۂ��̂ł̃T�C�Yx,y,z�����ւ��邱�ƂŃQ�[���I�u�W�F�N�g�ɍ��킹�Ă��
            if ((child.transform.localRotation.eulerAngles.x / 90) % 2 == 1)
            {
                Vector3 size = bounds.size;
                bounds.size = new Vector3(size.x, size.z, size.y);
            }
            if ((child.transform.localRotation.eulerAngles.y / 90) % 2 == 1)
            {
                Vector3 size = bounds.size;
                bounds.size = new Vector3(size.z, size.y, size.x);
            }
            if ((child.transform.localRotation.eulerAngles.z / 90) % 2 == 1)
            {
                Vector3 size = bounds.size;
                bounds.size = new Vector3(size.y, size.x, size.z);
            }

            // �ċA����
            bounds = CalcChildObjWorldBounds(child.gameObject, bounds);
        }
        //bounds.center += gameObject.transform.position;
        return bounds;
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