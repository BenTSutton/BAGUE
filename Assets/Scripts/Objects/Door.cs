using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Door linkedDoor;
    [SerializeField] private Transform teleportPoint;

    private Animator animator;
    private bool playerInRange;
    private GameObject player;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        animator.SetBool("openingDoor", playerInRange);

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TeleportPlayer();
        }
    }

    private void TeleportPlayer()
    {
        if (linkedDoor == null || player == null)
            return;

        player.transform.position = linkedDoor.teleportPoint.position;
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
}