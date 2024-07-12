using Flexalon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static PlasticGui.LaunchDiffParameters;

public class ObjectTable : MonoBehaviour
{
    [SerializeField] Vector3 elemetSize = new Vector3(0.7f, 0.7f, 0.2f);
    [SerializeField] float rowGap = 0.3f;
    [SerializeField] float colGap = 0.3f;

    [SerializeField] public List<GameObject> specimens;
    [SerializeField] public FlexalonGridLayout flexalonLayout;
    [SerializeField] public FlexalonObject flexalonObject;
    [SerializeField] public Transform backGround;


    private void Awake()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);

            if (child.name == "Specimen") specimens.Add(child.gameObject);
            else
            if (child.name == "Grid Layout")
            {
                flexalonLayout = child.GetComponent<FlexalonGridLayout>();
                flexalonObject = child.GetComponent<FlexalonObject>();
            }
            else
            if (child.name == "BackGround") backGround = child;
        }


        //foreach(var a in specimens) a.GetComponent<ChildObjBounds>().size = elemetSize;

        flexalonLayout.RowSpacing = rowGap;
        flexalonLayout.ColumnSpacing = colGap;
        flexalonObject.Scale = elemetSize;

        float backGroundX = colGap * flexalonLayout.Columns + elemetSize.x * flexalonLayout.Columns;
        float backGroundY = rowGap * flexalonLayout.Rows + elemetSize.y * flexalonLayout.Rows;
        backGround.localScale = new Vector3(backGroundX, backGroundY, 1);
        backGround.localPosition = new Vector3(0, 0, (elemetSize.z / 2) + 0.04f);
    }
}
