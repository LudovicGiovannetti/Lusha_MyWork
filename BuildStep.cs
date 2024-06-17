using Com.Lusha.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildStep : BuildingStep
{
    [SerializeField] private ParticleSystem buildingParticle = default;
    [SerializeField] private List<GameObject> blueprintPartList = new List<GameObject>();

    [field: Header("Parameters")]
    [field: SerializeField] public Item ItemNeededProperty { get; private set; } = default;


    public override void Action()
    {
        StartCoroutine(Build());
    }

    public override void Inialize()
    {
        count = objectLinked.Count;
        for (int i = 0; i < count; i++)
        {
            objectLinked[i].SetActive(false);
        }
    }

    public override void Skip()
    {
        base.Skip();
        count = objectLinked.Count;
        for (int i = 0; i < count; i++)
        {
            blueprintPartList[i].SetActive(false);
            objectLinked[i].SetActive(true);
        }
    }


    private IEnumerator Build()
    {
        for (int i = 0; i < count; i++)
        {
            blueprintPartList[i].SetActive(false);
            objectLinked[i].SetActive(true);
            yield return StartCoroutine(TweenUtils.Scale(objectLinked[i].transform, Vector3.zero, objectLinked[i].transform.localScale, actionDuration, curve));
            Instantiate(buildingParticle, objectLinked[i].transform.position, Quaternion.identity);
            yield return new WaitForSeconds(timeBetweenEachAction);
        }

        Complete();
    }

    [Serializable]
    public class ItemNeeded
    {
        [SerializeField] public string ItemID { get; private set; }
        [SerializeField] public int Quantity { get; private set; }
    }
}
