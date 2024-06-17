using Com.Lusha.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public delegate void BuildableEventHandler();
public class Buildable : MonoBehaviour
{
    [SerializeField] private List<BuildingStep> buildingSteps = new List<BuildingStep>();
    [SerializeField] private PopUpScript popUp = default;
    [SerializeField] private ConsumedItemPopUp consumedPopUpPrefab = default;
    [SerializeField] private GameObject popUpSpawnPos = default;
    [SerializeField] private GameObject blueprint;

    public BuildableEventHandler OnStepComplete;
    public BuildableEventHandler OnBuildFinish;
    public BuildableEventHandler OnSkipFinish;
    public int CurrentBuildStep { get; set; } = 0;

    private BuildingStep step;
    private InventoryManager inventoryManager;
    private bool requirement;

    //Apeller cette m?thode dans les diff?rents objets qui ont un component "Buildable"
    public void Init()
    {
        int count = buildingSteps.Count;
        for (int i = 0; i < count; i++)
        {
            buildingSteps[i].Inialize();
        }

        CheckForBuildStep();

        ResetStep();
        if (CurrentBuildStep <= buildingSteps.Count - 1)
            step = buildingSteps[CurrentBuildStep];
        inventoryManager = GameManager.instance.inventoryM;
    }

    public void Action()
    {
        GameManager.instance.LevelM.CurrentGameSceneManager.UIM.ButtonsAndJoysticks.SetActive(false);

        if (requirement)
            CheckForAction();
        else
        {
            StartCoroutine(popUp.ShakeAnimation());
            Vibration.Vibrate();

            GameManager.instance.LevelM.CurrentGameSceneManager.UIM.ButtonsAndJoysticks.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        popUp.ClosePopUp();
        ResetStep();
    }


    private void OnTriggerEnter(Collider other)
    {
        DisplayPopUp();
        CheckRequirement();
    }

    //Si l'?tape pr?cendente est une "CleanStep" on y retourner
    private void ResetStep()
    {
        if (CurrentBuildStep != 0 && buildingSteps[CurrentBuildStep - 1].GetComponent<CleanStep>())
        {
            StartCoroutine(buildingSteps[CurrentBuildStep - 1].GetComponent<CleanStep>().ResetCleanStep());
            CurrentBuildStep--;
            step = buildingSteps[CurrentBuildStep];
        }
    }

    //Permet de display la popup avec les bonnes informations en fonction du types de step
    private void DisplayPopUp()
    {
        popUp.gameObject.SetActive(true);
        popUp.OpenPopUp();

        if (step.GetComponent<BuildStep>())
        {
            BuildStep buildStep = step.GetComponent<BuildStep>();
            popUp.SetSprite(buildStep.ItemNeededProperty.IconID());
            popUp.SetText(inventoryManager.CheckItemQuantity(buildStep.ItemNeededProperty.itemID), buildStep.ItemNeededProperty.quantity);
        }
        else if (step.GetComponent<CleanStep>())
        {
            CleanStep cleanStep = step.GetComponent<CleanStep>();
            popUp.SetSprite(cleanStep.SpriteID);
        }
    }


    //M?thode li? ? l'event OnComplete que l'on trouve sur toute les ?tapes (classe : BuildingStep)
    private void BuildingStep_OnComplete()
    {
        GameManager.instance.LevelM.CurrentGameSceneManager.UIM.ButtonsAndJoysticks.SetActive(true);

        step.OnComplete -= BuildingStep_OnComplete;

        if (CurrentBuildStep <= buildingSteps.Count - 1)
        {
            CurrentBuildStep++;
        }

        if (CurrentBuildStep >= buildingSteps.Count)
        {
            blueprint.SetActive(false);
            Destroy(this);
            OnBuildFinish?.Invoke();
            return;
        }

        step = buildingSteps[CurrentBuildStep];
        OnStepComplete?.Invoke();

        DisplayPopUp();
        CheckRequirement();
    }

    //M?thode qui permet de passer les ?tapes d?j? faites, utilis?e dans la m?thode Init();
    private void CheckForBuildStep()
    {
        int count = buildingSteps.Count;
        int max = CurrentBuildStep;
        CurrentBuildStep = 0;

        for (int i = 0; i < count; i++)
        {
            if (i < max)
            {
                step = buildingSteps[i];
                step.OnSkip += BuildingStep_OnSkip;
                step.Skip();
            }
        }
    }

    private void BuildingStep_OnSkip()
    {
        step.OnSkip -= BuildingStep_OnSkip;

        if (CurrentBuildStep <= buildingSteps.Count - 1)
        {

            CurrentBuildStep++;
            Debug.Log(CurrentBuildStep);
        }

        if (CurrentBuildStep >= buildingSteps.Count)
        {
            blueprint.SetActive(false);
            Destroy(this);
            OnSkipFinish?.Invoke();
            return;
        }
    }

    private void CheckForAction()
    {
        int count = buildingSteps.Count;
        if (CurrentBuildStep <= count - 1)
        {
            popUp.ClosePopUp();

            BuildStep buildStep = step.GetComponent<BuildStep>() != null ? step.GetComponent<BuildStep>() : null;
            if (requirement && !step.GetComponent<CleanStep>())
            {
                ConsumedItemPopUp consumedPopUp = Instantiate(consumedPopUpPrefab, popUpSpawnPos.transform.position, Quaternion.identity);
                consumedPopUp.SetInformation(buildStep.ItemNeededProperty.IconID(), -buildStep.ItemNeededProperty.quantity);
                inventoryManager.ConsumeItem(buildStep.ItemNeededProperty.itemID, buildStep.ItemNeededProperty.quantity);

            }

            step.Action();
            step.OnComplete += BuildingStep_OnComplete;
        }
    }

    private void CheckRequirement()
    {
        BuildStep buildStep = step.GetComponent<BuildStep>() != null ? step.GetComponent<BuildStep>() : null;
        CleanStep cleanStep = step.GetComponent<CleanStep>() != null ? step.GetComponent<CleanStep>() : null;
        if ((buildStep
            && inventoryManager.CheckItemQuantity(buildStep.ItemNeededProperty.itemID) >= buildStep.ItemNeededProperty.quantity)
            || cleanStep)
        {
            requirement = true;
            popUp.ValidateAnimation();
        }
        else
        {
            requirement = false;
            popUp.RequirementNotMetAnimation();
        }
    }

}
