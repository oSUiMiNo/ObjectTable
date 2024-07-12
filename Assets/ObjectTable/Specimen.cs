using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// �q�I�u�W�F�N�g��W�{������
public class Specimen : MonoBehaviour
{
    /// <summary>
    /// �q�I�u�W�F�N�g�̓���Bounds
    /// </summary>
    Bounds childObjBounds;

    Transform layout => GameObject.Find("ObjectTable").transform.Find("Grid Layout");
    Transform backGround => transform.Find("BackGround");

    [SerializeField] bool createCollider;
    [SerializeField] public float size = 1f;
    [SerializeField] public Vector3 size3 = new Vector3(1, 1, 0.5f);


    void Start()
    {
        childObjBounds = CalcLocalObjBounds(gameObject);

        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.name == "BackGround") continue;
            children.Add(child);
        }

        // �W�{�ɂ��钆�g��������ΏI���
        if (children.Count == 0) return;

        // �L���[�u�̃T�C�Y�ƃ|�W�V�������o�E���f�B���O�{�b�N�X�ɍ��킹��
        // �q�I�u�W�F�N�g�ɉe�����Ȃ��悤�Ɉ�u�e�q�֌W����
        foreach (Transform child in children) child.transform.parent = null;
        gameObject.transform.localScale = childObjBounds.size + childObjBounds.size / 10;
        gameObject.transform.position = childObjBounds.center;
        foreach (Transform child in children) child.transform.parent = transform;

        float ratio = size / Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        gameObject.transform.localScale *= ratio;

        foreach (Transform child in children) child.transform.parent = null;
        gameObject.transform.localScale = Vector3.one * size;
        foreach (Transform child in children) child.transform.parent = transform;

        transform.parent = layout.transform;


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
            

            // �ċA����
            bounds = CalcChildObjWorldBounds(child.gameObject, bounds);
        }
        //bounds.center += gameObject.transform.position;
        return bounds;
    }
}