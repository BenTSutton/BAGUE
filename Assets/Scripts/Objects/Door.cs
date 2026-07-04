using UnityEngine;
using TMPro;

public class Door : MonoBehaviour
{
    [SerializeField] private Door linkedDoorLeft;
    [SerializeField] private Door linkedDoorRight;
    [SerializeField] private Transform teleportPoint;
    public string roomName;

    private Animator animator;
    public bool playerInRange;
    private GameObject player;
    private GameObject tooltipCanvasObj;
    private TMP_Text tooltipLeftText;
    private TMP_Text tooltipRightText;

    private void Start()
    {
        animator = GetComponent<Animator>();
        tooltipCanvasObj = transform.Find("Canvas").gameObject;
        tooltipLeftText = transform.Find("Canvas/TooltipLeft/RoomText").gameObject.GetComponent<TMP_Text>();
        tooltipRightText = transform.Find("Canvas/TooltipRight/RoomText").gameObject.GetComponent<TMP_Text>();
    }

    private void Update()
    {
        animator.SetBool("openingDoor", playerInRange);

        if(playerInRange)
        {
            ShowTooltip(true);
        }
        else
        {
            ShowTooltip(false);
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.Alpha1))
        {
            TeleportPlayerLeft();
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.Alpha2))
        {
            TeleportPlayerRight();
        }
    }

    private void TeleportPlayerLeft()
    {
        if (linkedDoorLeft == null || player == null)
            return;

        player.transform.position = linkedDoorLeft.teleportPoint.position;
    }
    private void TeleportPlayerRight()
    {
        if (linkedDoorRight == null || player == null)
            return;

        player.transform.position = linkedDoorRight.teleportPoint.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
        }
    }

    private void ShowTooltip(bool active)
    {
        tooltipLeftText.text = linkedDoorLeft.roomName;
        tooltipRightText.text = linkedDoorRight.roomName;
        tooltipCanvasObj.SetActive(active);
    }
}