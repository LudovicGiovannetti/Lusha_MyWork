using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void BuildingStepEventHandler();
public abstract class BuildingStep : MonoBehaviour
{
    [SerializeField] protected List<GameObject> objectLinked = new List<GameObject>();
    [SerializeField] protected AnimationCurve curve = default;
    [SerializeField] protected float timeBetweenEachAction = 0.3f;
    [SerializeField] protected float actionDuration = 0.3f;

    [Header("Particles")]
    [SerializeField] protected ParticleSystem finishParticle = default;

    public bool IsCompleted { get; protected set; }
    public BuildingStepEventHandler OnComplete;
    public BuildingStepEventHandler OnSkip;

    protected int count = 0;

    protected virtual void Start()
    {
        count = objectLinked.Count;
    }

    //Méthode qui permet de passer cette étape si elle est déjà faite. Elle peut être override dans les classes filles
    public virtual void Skip()
    {
        IsCompleted = true;
        OnSkip?.Invoke();
    }

    //Méthode abstraite qui doivent être abosulement implémentées dans les classes filles
    public abstract void Action();
    public abstract void Inialize();

    protected virtual void Complete()
    {
        OnComplete?.Invoke();   
        IsCompleted = true;
        finishParticle.Play();
    }
}
