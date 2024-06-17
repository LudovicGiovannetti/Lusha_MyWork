using Com.Lusha.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanStep : BuildingStep
{
    [SerializeField] private GameObject blueprint = default;
    [SerializeField] private GameObject objectLinkedParent = default;
    [Space(10)]
    [Header("Curves")]
    [SerializeField] private AnimationCurve moveBlueprintCurve = default;
    [SerializeField] private AnimationCurve scaleBlueprintCurve = default;
    [Space(10)]
    [Header("Blueprint Juiciness settings")]
    [SerializeField] private float maxScaleSizeForBlueprint = 1.5f;
    [SerializeField] private float bumpDurationForBlueprint = 0.05f;
    [SerializeField] private float blueprintMoveDuration = 0.05f;
    [SerializeField] private float blueprintYOffset = 0.1f;
    [Space(10)]
    [Header("Other Juiciness settings")]
    [SerializeField] private float scaleDuration = 0.1f;
    [Space(10)]
    [Header("Camera Shake settings")]
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeIntensity = 0.1f;

    [field: Space(10)]
    [field: Header("Icon")]
    [field: SerializeField] public string SpriteID { get; private set; }

    private List<Vector3> objectSize;
    private Vector3 blueprintObjectSize;
    private Vector3 objectLinkedParentSize;

    public override void Action()
    {
        StartCoroutine(Clean());
    }

    public override void Inialize()
    {
        for (int i = 0; i < count; i++)
        {
            objectLinked[i].SetActive(true);
        }
    }

    public override void Skip()
    {
        base.Skip();
        for (int i = 0; i < count; i++)
        {
            objectLinked[i].SetActive(false);
        }
        blueprint.SetActive(true);
    }

    public IEnumerator ResetCleanStep()
    {
        yield return StartCoroutine(TweenUtils.Scale(blueprint.transform, blueprint.transform.localScale, Vector3.zero, scaleDuration, scaleBlueprintCurve));
        Inialize();
        yield return StartCoroutine(TweenUtils.Scale(objectLinkedParent.transform, Vector3.zero, objectLinkedParent.transform.localScale, scaleDuration, scaleBlueprintCurve));
    }

    private IEnumerator Clean()
    {
        Vector3 localScale;
        for (int i = 0; i < count; i++)
        {
            localScale = objectLinked[i].transform.localScale;
            yield return StartCoroutine(TweenUtils.Scale(objectLinked[i].transform, localScale, Vector3.zero, actionDuration, curve));
            objectLinked[i].SetActive(false);
            objectLinked[i].transform.localScale = localScale;
            yield return new WaitForSeconds(timeBetweenEachAction);
        }

        Complete();
    }

    protected override void Complete()
    {
        StartCoroutine(SpawnBlueprint());
        IEnumerator SpawnBlueprint()
        {
            Vector3 startPosition = blueprint.transform.localPosition;
            blueprint.transform.localPosition = startPosition + Vector3.up * blueprintYOffset;
            blueprint.SetActive(true);
            yield return StartCoroutine(TweenUtils.Scale(blueprint.transform, Vector3.zero, Vector3.one, scaleDuration, scaleBlueprintCurve));
            yield return StartCoroutine(TweenUtils.Move(blueprint.transform, blueprint.transform.localPosition, startPosition, blueprintMoveDuration, moveBlueprintCurve));
            CameraManager.Instance.LaunchCameraShake(shakeIntensity, shakeDuration);
            yield return StartCoroutine(TweenUtils.Scale(blueprint.transform, Vector3.one, Vector3.one * maxScaleSizeForBlueprint, bumpDurationForBlueprint, scaleBlueprintCurve));
            yield return StartCoroutine(TweenUtils.Scale(blueprint.transform, Vector3.one * maxScaleSizeForBlueprint, Vector3.one, bumpDurationForBlueprint, scaleBlueprintCurve));
            base.Complete();
        }
    }

}
