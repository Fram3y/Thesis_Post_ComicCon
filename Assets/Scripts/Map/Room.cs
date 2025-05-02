using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private GameObject _clearedIcon;

    [Header("Room Doors")]
    [SerializeField] private GameObject _topDoor;
    [SerializeField] private GameObject _bottomDoor;
    [SerializeField] private GameObject _leftDoor;
    [SerializeField] private GameObject _rightDoor;

    [Header("Room Walls")]
    [SerializeField] private GameObject _topWall;
    [SerializeField] private GameObject _bottomWall;
    [SerializeField] private GameObject _leftWall;
    [SerializeField] private GameObject _rightWall;

    public Vector2Int RoomIndex { get; set; }
    private RoomManager _roomManager;
    public int _enemyCount;

    private bool _isProcessing = false;

    void Awake()
    {
        _roomManager = FindObjectOfType<RoomManager>();

        // Check to make sure Room Manager is found
        if (_roomManager == null)
        {
            Debug.LogError("RoomManager not found in the scene.");
        }
    }

    void Start()
    {
        InstantiateWall(Vector2Int.up);
        InstantiateWall(Vector2Int.down);
        InstantiateWall(Vector2Int.left);
        InstantiateWall(Vector2Int.right);
    }

    void CheckRoomCleared()
    {
        if (_enemyCount == 0 && _clearedIcon != null)
        {
            _clearedIcon.SetActive(true);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (this == null || gameObject == null) return; // Prevent access if destroyed

        if (_isProcessing) return; 
        _isProcessing = true;

        if (other.CompareTag("Enemy"))
        {
            _enemyCount += 1;
        }
        else if (other.CompareTag("Player"))
        {
            if (_enemyCount > 0)
            {
                CloseDoors();
            }
            else
            {
                if (_roomManager != null && gameObject != null)
                {
                    _roomManager.OpenDoors(gameObject, RoomIndex.x, RoomIndex.y);
                    CheckRoomCleared(); // Check cleared status when the player enters and no enemies are present
                }
            }
        }

        _isProcessing = false;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (this == null || gameObject == null) return;

        if (_isProcessing) return;
        _isProcessing = true;

        if (other.CompareTag("Enemy"))
        {
            _enemyCount -= 1;
            _roomManager._totalEnemyCount -= 1;
            if (_enemyCount == 0)
            {
                if (_roomManager != null && gameObject != null)
                {
                    _roomManager.OpenDoors(gameObject, RoomIndex.x, RoomIndex.y);
                    CheckRoomCleared(); // Check room cleared status on enemy exit
                }
            }
        }
        else if (other.CompareTag("Player"))
        {
            if (_enemyCount == 0)
            {
                if (_roomManager != null && gameObject != null)
                {
                    _roomManager.OpenDoors(gameObject, RoomIndex.x, RoomIndex.y);
                    CheckRoomCleared(); // Check room cleared status when the player exits
                }
            }
        }

        _isProcessing = false;
    }

    public void CloseDoors()
    {
        _topDoor.SetActive(false);
        _bottomDoor.SetActive(false);
        _leftDoor.SetActive(false);
        _rightDoor.SetActive(false);

        InstantiateWall(Vector2Int.up);
        InstantiateWall(Vector2Int.down);
        InstantiateWall(Vector2Int.left);
        InstantiateWall(Vector2Int.right);

        // PlayCloseDoor();
    }

    public void OpenDoor(Vector2Int direction)
    {
        if (direction == Vector2Int.up)
        {
            _topDoor.SetActive(true);
            _topWall.SetActive(false);
        }

        if (direction == Vector2Int.down)
        {
            _bottomDoor.SetActive(true);
            _bottomWall.SetActive(false);
        }

        if (direction == Vector2Int.left)
        {
            _leftDoor.SetActive(true);
            _leftWall.SetActive(false);
        }

        if (direction == Vector2Int.right)
        {
            _rightDoor.SetActive(true);
            _rightWall.SetActive(false);
        }

        // PlayOpenDoor();
    }

    public void InstantiateWall(Vector2Int direction)
    {
        if (direction == Vector2Int.up && !_topDoor.activeSelf)
        {
            _topWall.SetActive(true);
        }

        if (direction == Vector2Int.down && !_bottomDoor.activeSelf)
        {
            _bottomWall.SetActive(true);
        }

        if (direction == Vector2Int.left && !_leftDoor.activeSelf)
        {
            _leftWall.SetActive(true);
        }

        if (direction == Vector2Int.right && !_rightDoor.activeSelf)
        {
            _rightWall.SetActive(true);
        }
    }
}