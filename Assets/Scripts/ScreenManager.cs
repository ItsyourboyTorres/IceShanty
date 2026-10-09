using UnityEngine;
using Unity.Cinemachine;

public sealed class ScreenManager : MonoBehaviour
{
    public enum Screen { Shop, Fishing, Sell }

    [SerializeField] CinemachineCamera shop, fishing, sell;
    [SerializeField] CinemachineBrain brain;

    public bool InputLocked { get; set; }
    public bool IsTransitioning => brain && brain.IsBlending;

    public Screen Current { get; private set; } = Screen.Fishing;

    void Awake()
    {
        if (brain) brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0.85f);
        SetScreen(Screen.Fishing);
    }

    public void GoLeft() => Move(-1);
    public void GoRight() => Move(1);
    public void ReturnToFishing() => SetScreen(Screen.Fishing);

    void Move(int direction)
    {
        if (InputLocked || IsTransitioning) return;
        var next = (Screen)Mathf.Clamp((int)Current + direction, 0, 2);
        if (next != Current) SetScreen(next);
    }

    void SetScreen(Screen screen)
    {
        Current = screen;
        shop.Priority = screen == Screen.Shop ? 10 : 0;
        fishing.Priority = screen == Screen.Fishing ? 10 : 0;
        sell.Priority = screen == Screen.Sell ? 10 : 0;
    }
}



