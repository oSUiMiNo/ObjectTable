using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 子オブジェクトを標本化する
public class Specimen : MonoBehaviour
{
    /// <summary>
    /// 子オブジェクトの統合Bounds
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

        // 標本にする中身が無ければ終わり
        if (children.Count == 0) return;

        // キューブのサイズとポジションをバウンディングボックスに合わせる
        // 子オブジェクトに影響しないように一瞬親子関係解消
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


        // Boundsの大きさと形状が見た目に分かるようコライダーを追加する
        if (createCollider)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        }
    }


    /// <summary>
    /// 現在オブジェクトのローカル座標でのバウンド計算
    /// </summary>
    private Bounds CalcLocalObjBounds(GameObject obj)
    {
        // 指定オブジェクトのワールドバウンドを計算する
        Bounds totalBounds = CalcChildObjWorldBounds(obj, new Bounds());

        // ローカルオブジェクトの相対座標に合わせてバウンドを再計算する
        // オブジェクトのワールド座標とサイズを取得する
        Vector3 ObjWorldPosition = transform.position;
        Vector3 ObjWorldScale = transform.lossyScale;

        // バウンドのローカル座標とサイズを取得する
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
    /// 子オブジェクトのワールド座標でのバウンド計算（再帰処理）
    /// </summary>
    private Bounds CalcChildObjWorldBounds(GameObject obj, Bounds bounds)
    {
        // 指定オブジェクトの全ての子オブジェクトをチェックする
        foreach (Transform child in obj.transform)
        {
            if (child.name == "BackGround") continue;

            // メッシュフィルタの存在確認
            MeshFilter filter = child.gameObject.GetComponent<MeshFilter>();

            // スキンドメッシュレンダラの存在確認
            SkinnedMeshRenderer skinnedRenderer = child.gameObject.GetComponent<SkinnedMeshRenderer>();

            Bounds meshBounds = new Bounds();

            if (filter != null)
            {
                // フィルターのメッシュ情報からバウンドボックスを取得する
                meshBounds = filter.mesh.bounds;
            }
            else
            if (skinnedRenderer != null)
            {
                // スキンドメッシュレンダラからバウンドボックスを取得する
                meshBounds = skinnedRenderer.sharedMesh.bounds;
            }
            else
            {
                // 再帰処理
                bounds = CalcChildObjWorldBounds(child.gameObject, bounds);
                continue;
            }

            // オブジェクトのワールド座標とサイズを取得する
            Vector3 ObjWorldPosition = child.position;
            Vector3 ObjWorldScale = child.lossyScale;

            // バウンドのワールド座標とサイズを取得する
            Vector3 meshBoundsWorldCenter = meshBounds.center + ObjWorldPosition;
            Vector3 meshBoundsWorldSize = Vector3.Scale(meshBounds.size, ObjWorldScale);

            // バウンドの最小座標と最大座標を取得する
            Vector3 meshBoundsWorldMin = meshBoundsWorldCenter - (meshBoundsWorldSize / 2);
            Vector3 meshBoundsWorldMax = meshBoundsWorldCenter + (meshBoundsWorldSize / 2);

            // 取得した最小座標と最大座標を含むように拡大/縮小を行う
            if (bounds.size == Vector3.zero)
            {
                // 元バウンドのサイズがゼロの場合はバウンドを作り直す
                bounds = new Bounds(meshBoundsWorldCenter, Vector3.zero);
            }
            bounds.Encapsulate(meshBoundsWorldMin);
            bounds.Encapsulate(meshBoundsWorldMax);
            

            // 再帰処理
            bounds = CalcChildObjWorldBounds(child.gameObject, bounds);
        }
        //bounds.center += gameObject.transform.position;
        return bounds;
    }
}