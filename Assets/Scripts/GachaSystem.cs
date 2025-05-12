using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GachaSystem : MonoBehaviour
{
    public Transform resultRoot;
    public GameObject slotPrefab;
    public GachaDatabase gachaDB;

    public void StartReveal()
    {

        PlayerCurrency.Instance.RequestSpendDiamond("dd", success =>
        {
            if (success)
            {
                List<GachaItem> results = GetRandomItems(10);
                StartCoroutine(ShowResultsRoutine(results));
            }
            else
            {
                Debug.LogError("���̾ư� �����մϴ�");
            }
        });
    }

    private List<GachaItem> GetRandomItems(int count)
    {
        List<GachaItem> result = new List<GachaItem>();
        int totalWeight = gachaDB.items.Sum(i => i.weight);

        for (int i = 0; i < count; i++)
        {
            int rand = Random.Range(0, totalWeight);
            int current = 0;

            foreach (var item in gachaDB.items)
            {
                current += item.weight;
                if (rand < current)
                {
                    result.Add(item);
                    break;
                }
            }
        }

        return result;
    }

    private IEnumerator ShowResultsRoutine(List<GachaItem> results)
    {
        List<GameObject> slots = new List<GameObject>();

        // Step 1: ������ ���� ���� (Shine ����)
        for (int i = 0; i < results.Count; i++)
        {
            GameObject obj = Instantiate(slotPrefab, resultRoot);
            var slot = obj.GetComponent<GachaResultSlot>();
            slot.SetupShineOnly();
            slots.Add(obj);
            yield return new WaitForSeconds(0.05f); // �ٴٴٴ� ȿ��
        }

        yield return new WaitForSeconds(0.5f);

        // Step 2: �ϳ��� Reveal
        for (int i = 0; i < results.Count; i++)
        {
            var slot = slots[i].GetComponent<GachaResultSlot>();
            slot.Reveal(results[i]);
            yield return new WaitForSeconds(0.2f);
        }
    }
}