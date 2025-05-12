using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class AutoGridCellSize : MonoBehaviour
{
    public int columnCount = 3; // 한 줄에 몇 개?
    public Vector2 padding = new Vector2(40, 40); // 좌우 여백
    public Vector2 spacing = new Vector2(30, 30); // 셀 간 간격

    private RectTransform rt;
    private GridLayoutGroup grid;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        grid = GetComponent<GridLayoutGroup>();
        UpdateCellSize();
    }

    void UpdateCellSize()
    {
        float totalWidth = rt.rect.width;
        float totalSpacing = spacing.x * (columnCount - 1);
        float totalPadding = padding.x * 2;

        float cellWidth = (totalWidth - totalSpacing - totalPadding) / columnCount;
        float cellHeight = cellWidth * 1.4f; // 비율 지정 (ex. 4:5 비율)

        grid.cellSize = new Vector2(cellWidth, cellHeight);
        grid.spacing = spacing;
        grid.padding.left = (int)padding.x;
        grid.padding.right = (int)padding.x;
        grid.padding.top = (int)padding.y;
        grid.padding.bottom = (int)padding.y;
    }
}
