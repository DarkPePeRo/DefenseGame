using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcqisitionManager : MonoBehaviour
{
    public static AcqisitionManager Instance;
    public Transform acqisitionContent;
    public GameObject topSpacerPrefab;
    private Transform topSpacerInstance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        EnsureTopSpacer();
    }

    void Update()
    {

    }
    private void EnsureTopSpacer()
    {
        if (topSpacerInstance != null) return;
        var go = Instantiate(topSpacerPrefab, acqisitionContent, false);
        topSpacerInstance = go.transform;
        topSpacerInstance.SetAsFirstSibling();
    }
    public void AcReward()
    {

    }
}
