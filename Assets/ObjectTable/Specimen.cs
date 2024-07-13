using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlasticGui.LaunchDiffParameters;


// 子オブジェクトを標本化する
public class Specimen : MonoBehaviour
{
    /// <summary>
    /// 子オブジェクトの統合Bounds
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

        // 標本にする中身が無ければ終わり
        if (children.Count == 0) return;

        // キューブのサイズとポジションをバウンディングボックスに合わせる
        // 子オブジェクトに影響しないように一瞬親子関係解消
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

        // ピボットがメッシュの中心に無いゲームオブジェクトでも標本の真ん中に配置されるようにする
        // Center座標を求める
        Vector3 humanCenterPos = GetCenterPosition(body);
        // bodyをどれだけ動かすと、Pivotの位置にCenterを持ってこられるか求める
        Vector3 centerDis = body.position - humanCenterPos;
        // 標本硝子の座標と、bodyのPivotとCenterの位置の差を足せば完了
        body.position = transform.position + centerDis;


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

            Debug.Log($"{child} {child.transform.localRotation.eulerAngles}");

            // ゲームオブジェクトの向きが実際のメッシュの向きから90度単位で回転していた場合、
            // 実際のメッシュのバウンディングボックスを使うと、シーンに配置されたゲームオブジェクトと向きが違ってしまう
            // バウンディングボックスは回転できないっぽいのでのサイズx,y,zを入れ替えることでゲームオブジェクトに合わせてやる
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

            // 再帰処理
            bounds = CalcChildObjWorldBounds(child.gameObject, bounds);
        }
        //bounds.center += gameObject.transform.position;
        return bounds;
    }


    public static Vector3 GetCenterPosition(Transform target)
    {
        //非アクティブも含めて、targetとtargetの子全てのレンダラーとコライダーを取得
        var cols = target.GetComponentsInChildren<Collider>(true);
        var rens = target.GetComponentsInChildren<Renderer>(true);

        //コライダーとレンダラーが１つもなければ、target.positionがcenterになる
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

            //最初の１度だけ通って、minPosとmaxPosを初期化する
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

            //コライダーが１つもなければ１度だけ通って、minPosとmaxPosを初期化する
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