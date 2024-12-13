using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    [SerializeField] DialogManager dialogManager;
    [SerializeField] CameraAnimationsController cameraAnimationsController;
    [SerializeField] CharSelectorScript charSelector;

    CharacterController controller = null;

    public void PauseGame()
    {
        if (controller == null) { controller = GetComponentInChildren<CharacterController>(); }
        controller.gamePaused = true;
        dialogManager.ShowPause();
    }

    public void UnpauseGame()
    {
        if (controller == null) { controller = GetComponentInChildren<CharacterController>(); }
        dialogManager.HidePause();
        controller.gamePaused = false;
    }

    public void ChangeCharacter()
    {
        if (controller == null) { controller = GetComponentInChildren<CharacterController>(); }
        controller.ClearTheGame();
        dialogManager.HidePause();
        dialogManager.HideEndScreen();
        cameraAnimationsController.ResetCameraPosition();
        charSelector.StartSelection();
    }

    public void Restart()
    {
        if (controller == null) { controller = GetComponentInChildren<CharacterController>(); }
        controller.ClearTheGame();
        dialogManager.HidePause();
        dialogManager.HideEndScreen();
        cameraAnimationsController.ResetCameraPosition();
        charSelector.SetTheSameCharacterToGame();
    }
}
