using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomManager : MonoBehaviour
{
    [SerializeField] GameObject _roomPrefab;
    [SerializeField] GameObject _enemyPrefab;
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] GameObject _healthPotionPrefab;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private GameObject _regenTimerObject;
    
    [SerializeField] private int _maxRooms = 15;
    [SerializeField] private int _minRooms = 10;

    int _roomWidth = 20;
    int _roomHeight = 12;

    [SerializeField] int _gridSizeX = 15;
    [SerializeField] int _gridSizeY = 15;

    private List<GameObject> _roomObjects = new List<GameObject>();
    private Queue<Vector2Int> _roomQueue = new Queue<Vector2Int>();
    private int[,] _roomGrid;

    public bool generationComplete = false;
    private int _roomCount;

    private GameObject _player;
    public int _totalEnemyCount = 0;
    private float regenerationDelay = 10f;
    private float currentTime;
    private Coroutine regenerationCoroutine;

    private void Start()
    {
        currentTime = regenerationDelay;
        _regenTimerObject.SetActive(false);
        RegenerateAllRooms(); // Start by generating all rooms
    }

    private void Update()
    {
        if (_roomQueue.Count > 0 && _roomCount < _maxRooms && !generationComplete)
        {
            Vector2Int roomIndex = _roomQueue.Dequeue();
            int gridX = roomIndex.x;
            int gridY = roomIndex.y;

            TryGenerateRoom(new Vector2Int(gridX - 1, gridY));
            TryGenerateRoom(new Vector2Int(gridX + 1, gridY));
            TryGenerateRoom(new Vector2Int(gridX, gridY + 1));
            TryGenerateRoom(new Vector2Int(gridX, gridY - 1));
        } 
        else if (_roomCount < _minRooms)
        {
            Debug.Log("Invalid Room Count, Regenerating...");
            RegenerateAllRooms(); // Regenerate everything if the room count is invalid
        } 
        else if (!generationComplete)
        {
            Debug.Log($"Generation Complete, {_roomCount} rooms created");
            generationComplete = true;
        }

        if (_totalEnemyCount <= 0)
        {
            if (!_regenTimerObject.activeSelf)  // If the timer isn't already visible, show it
            {
                _regenTimerObject.SetActive(true);
            }

            RegenerationTimer();
        }
    }

    private void RegenerateAllRooms()
    {
        _roomObjects.RemoveAll(r => r == null); // Remove destroyed rooms
        
        // Destroy all existing rooms
        foreach (var room in _roomObjects)
        {
            Destroy(room);
        }

        // Clear the room list, grid, and queue
        _roomObjects.Clear();
        _roomGrid = new int[_gridSizeX, _gridSizeY];
        _roomQueue.Clear();
        _roomCount = 0;
        generationComplete = false;
        _totalEnemyCount = 0;

        // Generate the first room
        Vector2Int initialRoomIndex = new Vector2Int(_gridSizeX / 2, _gridSizeY / 2);
        StartRoomGenerationFromRoom(initialRoomIndex);

        // Spawn or move the player to the first room
        MovePlayerToFirstRoom();
    }

    private void RegenerationTimer()
    {
        // Count down the timer when enemies are dead
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;  // Decrease time each frame
            UpdateTimerUI();  // Update the UI with the new time
        }
        else
        {
            RegenerateAllRooms();  // Once the timer reaches 0, regenerate the rooms
            _regenTimerObject.SetActive(false);

            currentTime = regenerationDelay;
        }
    }

    private void UpdateTimerUI()
    {
        // Format the remaining time and update the timer text
        _timerText.text = $"Regenerating in: {Mathf.Ceil(currentTime)}s";
    }

    private void StartRoomGenerationFromRoom(Vector2Int roomIndex)
    {
        _roomQueue.Enqueue(roomIndex);
        int x = roomIndex.x;
        int y = roomIndex.y;
        _roomGrid[x, y] = 1;
        _roomCount++;

        var initialRoom = Instantiate(_roomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
        initialRoom.name = $"Room-{_roomCount}";
        initialRoom.GetComponent<Room>().RoomIndex = roomIndex;
        _roomObjects.Add(initialRoom);

        // First room has no enemies
        SpawnEnemies(initialRoom, 0);
    }

    private bool TryGenerateRoom(Vector2Int roomIndex)
    {
        int x = roomIndex.x;
        int y = roomIndex.y;

        if (_roomCount >= _maxRooms) return false;
        if (Random.value < 0.5f && roomIndex != Vector2Int.zero) return false;
        if (CountAdjacentRooms(roomIndex) > 1) return false;
        if (_roomGrid[x, y] != 0) return false;

        _roomQueue.Enqueue(roomIndex);
        _roomGrid[x, y] = 1;
        _roomCount++;

        var newRoom = Instantiate(_roomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
        newRoom.GetComponent<Room>().RoomIndex = roomIndex;
        newRoom.name = $"Room-{_roomCount}";
        _roomObjects.Add(newRoom);

        OpenDoors(newRoom, x, y);

        // Determine enemy count based on room number
        int enemyCount = GetEnemyCountForRoom(_roomCount);
        SpawnEnemies(newRoom, enemyCount);

        // Spawn HealthPotion in every 3rd room and the final room
        if (_roomCount % 3 == 0 || _roomCount == _maxRooms)
        {
            SpawnHealthPotion(newRoom);
        }

        return true;
    }

    // Function to spawn a HealthPotion in the room
    private void SpawnHealthPotion(GameObject room)
    {
        if (_healthPotionPrefab == null) return;

        // Get a random position within the room bounds
        Vector3 spawnPosition = GetRandomPositionInRoom(room.transform.position);
        GameObject healthPotion = Instantiate(_healthPotionPrefab, spawnPosition, Quaternion.identity, room.transform);

        // Set the health potion's sprite renderer to the "Player" sorting layer
        SpriteRenderer healthPotionRenderer = healthPotion.GetComponent<SpriteRenderer>();
        if (healthPotionRenderer != null)
        {
            healthPotionRenderer.sortingLayerName = "Player";
        }
    }

    private void MovePlayerToFirstRoom()
    {
        if (_roomObjects.Count == 0)
        {
            Debug.LogError("No rooms available to place the player.");
            return;
        }

        // Get the first room
        GameObject firstRoom = _roomObjects[0];
        Vector3 playerSpawnPosition = firstRoom.transform.position;

        // Spawn or move the player
        if (_player == null)
        {
            _player = Instantiate(_playerPrefab, playerSpawnPosition, Quaternion.identity);
        }
        else
        {
            _player.transform.position = playerSpawnPosition;
        }

        Debug.Log("Player moved to the first room.");
    }

    // Function to determine how many enemies should spawn per room
    private int GetEnemyCountForRoom(int roomNumber)
    {
        if (roomNumber == 1) return 0;  // First room has no enemies
        if (roomNumber <= 4) return 2;  // Rooms 2-4 have 1 enemy
        if (roomNumber <= 7) return 3;  // Rooms 5-7 have 2 enemies
        return 4;  // Rooms 8+ have 3 enemies
    }

    private void SpawnEnemies(GameObject room, int enemyCount)
    {
        if (_enemyPrefab == null) return;

        if (enemyCount > 0)
        {
            _regenTimerObject.SetActive(false);
        }

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = GetRandomPositionInRoom(room.transform.position);
            GameObject enemy = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity, room.transform);

            // Ensure the enemy is on the correct sorting layer
            SpriteRenderer enemyRenderer = enemy.GetComponent<SpriteRenderer>();
            if (enemyRenderer != null)
            {
                enemyRenderer.sortingLayerName = "Player";
            }

            _totalEnemyCount++;
        }
    }

    // Get a random position within the room bounds
    private Vector3 GetRandomPositionInRoom(Vector3 roomPosition)
    {
        float marginX = _roomWidth * 0.3f; // Margin to avoid door areas
        float marginY = _roomHeight * 0.3f;

        float minX = roomPosition.x - (_roomWidth / 2) + marginX;
        float maxX = roomPosition.x + (_roomWidth / 2) - marginX;
        float minY = roomPosition.y - (_roomHeight / 2) + marginY;
        float maxY = roomPosition.y + (_roomHeight / 2) - marginY;

        float spawnX = Random.Range(minX, maxX);
        float spawnY = Random.Range(minY, maxY);

        return new Vector3(spawnX, spawnY, 0);
    }

    public void OpenDoors(GameObject room, int x, int y)
    {
        Room newRoomScript = room.GetComponent<Room>();

        // Only open doors if the current room has 0 enemies
        if (newRoomScript._enemyCount == 0)
        {
            // Neighbors
            Room leftRoomScript = GetRoomScriptAt(new Vector2Int(x - 1, y));
            Room rightRoomScript = GetRoomScriptAt(new Vector2Int(x + 1, y));
            Room topRoomScript = GetRoomScriptAt(new Vector2Int(x, y + 1));
            Room bottomRoomScript = GetRoomScriptAt(new Vector2Int(x, y - 1));

            // Open doors to immediate neighbors based on available neighbors
            if (x > 0 && _roomGrid[x - 1, y] != 0 && leftRoomScript != null)
            {
                newRoomScript.OpenDoor(Vector2Int.left);
                leftRoomScript.OpenDoor(Vector2Int.right);
            }
            if (x < _gridSizeX - 1 && _roomGrid[x + 1, y] != 0 && rightRoomScript != null)
            {
                newRoomScript.OpenDoor(Vector2Int.right);
                rightRoomScript.OpenDoor(Vector2Int.left);
            }
            if (y > 0 && _roomGrid[x, y - 1] != 0 && bottomRoomScript != null)
            {
                newRoomScript.OpenDoor(Vector2Int.down);
                bottomRoomScript.OpenDoor(Vector2Int.up);
            }
            if (y < _gridSizeY - 1 && _roomGrid[x, y + 1] != 0 && topRoomScript != null)
            {
                newRoomScript.OpenDoor(Vector2Int.up);
                topRoomScript.OpenDoor(Vector2Int.down);
            }
        }
    }

    private Room GetRoomScriptAt(Vector2Int index)
    {
        GameObject room = _roomObjects.Find(r => r != null && r.GetComponent<Room>().RoomIndex == index);

        if (room == null)
        {
            return null;
        }

        return room.GetComponent<Room>();
    }

    private int CountAdjacentRooms(Vector2Int roomIndex)
    {
        int x = roomIndex.x;
        int y = roomIndex.y;
        int count = 0;

        if (x > 0 && _roomGrid[x - 1, y] != 0) count++; // Left Neighbour
        if (x < _gridSizeX - 1 && _roomGrid[x + 1, y] != 0) count++; // Right Neighbour
        if (y > 0 && _roomGrid[x, y -1] != 0) count++; // Neighbour Below
        if (y < _gridSizeY - 1 && _roomGrid[x, y + 1] != 0) count++; // Neighbour Above

        return count;
    }

    private Vector3 GetPositionFromGridIndex(Vector2Int gridIndex)
    {
        int gridX = gridIndex.x;
        int gridY = gridIndex.y;

        return new Vector3(_roomWidth * (gridX - _gridSizeX / 2), _roomHeight * (gridY - _gridSizeY / 2));
    }

    private void OnDrawGizmos()
    {
        Color gizmoColor = new Color(0, 1, 1, 0.05f);
        Gizmos.color = gizmoColor;

        for (int x = 0; x < _gridSizeX; x++)
        {
            for (int y = 0; y < _gridSizeY; y++)
            {
                Vector3 position = GetPositionFromGridIndex(new Vector2Int(x, y));
                Gizmos.DrawWireCube(position, new Vector3(_roomWidth, _roomHeight, 1));
            }
        }
    }
}