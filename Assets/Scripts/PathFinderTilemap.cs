using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Node
{
    private Vector3Int _position;
    private Vector3Int _targetPosition;
    private Node _parent;
    private int _costForMove;
    private int _fullPathCost;
    private int _fromStartCost;
    private int _toTargetCost;

    public int fullPathCost => _fullPathCost;
    public Vector3Int position => _position;
    public Vector3Int targetPosition => _targetPosition;
    public Node parent => _parent;

    public Node(Vector3Int position, Vector3Int targetPosition, Node parent, int costForMove = 1)
    {
        _position = position;
        _targetPosition = targetPosition;
        _costForMove = costForMove;
        _parent = parent;
        if (parent != null) CalculatePathCosts();
    }

    private void CalculatePathCosts()
    {
        _fromStartCost = _parent._fromStartCost + _parent._costForMove;
        _toTargetCost = Mathf.Abs(_targetPosition.x - _position.x) + Mathf.Abs(_targetPosition.y - _position.y);
        _fullPathCost = _fromStartCost + _toTargetCost;
    }
}

public class PathFinderTilemap : MonoBehaviour
{
    [SerializeField] private Tilemap _impassableTilemap;
    [SerializeField] private int _maxCountCheckedNode = 300;
    
    [Header("Settings Gizmo")] 
    [SerializeField] private bool _drawGizmo;
    [SerializeField] private Color _checkedNodeColor;
    [SerializeField] private Color _pathColor;
    
    private Vector3 _tileAnchor;
    private List<Vector2> _path;
    private Dictionary<Vector3Int, Node> _checkedNodes;
    private List<Node> _uncheckedNodes;
    private Vector3Int _target;

    private Vector3Int positionInt => Vector3Int.FloorToInt(transform.position);

    private void Start()
    {
        _tileAnchor = _impassableTilemap.tileAnchor;
    }

    public List<Vector2> GetPathToTarget(Vector3Int target)
    {
        ResetСurrentPath();
        if (target == positionInt) return _path;
        
        _target = target;
        CalculatePathToTarget();
        return _path;
    }

    private void ResetСurrentPath()
    {
        _path = new List<Vector2>();
        _checkedNodes = new Dictionary<Vector3Int, Node>();
        _uncheckedNodes = new List<Node>();
    }
    
    private void CalculatePathToTarget()
    {
        Node startNode = CreateStartNode();
        AddNodeNeighboursToCheck(startNode);
        CheckUnchekedNodes();
    }

    private Node CreateStartNode()
    {
        return new Node(positionInt, _target, null);
    }
    
    private void AddNodeNeighboursToCheck(Node node)
    {
        bool nodeIsChecked = _checkedNodes.ContainsKey(node.position);
        if (!nodeIsChecked)
        {
            _checkedNodes.Add(node.position, node);
            _uncheckedNodes.AddRange(GetNeighbourNodes(node));
        }
    }
    
    private List<Node> GetNeighbourNodes(Node node)
    {
        List<Node> neighbours = new List<Node>()
        {
            new Node(
                node.position + Vector3Int.right,
                node.targetPosition,
                node),

            new Node(
                node.position + Vector3Int.left,
                node.targetPosition,
                node),

            new Node(
                node.position + Vector3Int.up,
                node.targetPosition,
                node),

            new Node(
                node.position + Vector3Int.down,
                node.targetPosition,
                node)
        };

        return neighbours;
    }
    
    private void CheckUnchekedNodes()
    {
        int countCheckedNode = 0;
        while (_path.Count == 0 && countCheckedNode < _maxCountCheckedNode)
        {
            countCheckedNode++;
            Node checkingNode = GetCheapestUncheckedNode();
            bool canMove = !_impassableTilemap.HasTile(checkingNode.position);
            
            if (canMove)
            {
                AddNodeNeighboursToCheck(checkingNode);
            }
            CheckNode(checkingNode);
        }
    }

    private Node GetCheapestUncheckedNode()
    {
        Node cheapestNode = _uncheckedNodes[0];
        foreach (var node in _uncheckedNodes)
        {
            if (node.fullPathCost < cheapestNode.fullPathCost)
            {
                cheapestNode = node;
            }
        }

        return cheapestNode;
    }

    private void CheckNode(Node node)
    {
        if (node.position == _target)
        {
            _path = CreatePathFromNode(node);
            return;
        }

        AddNodeToChecked(node);
        UpdateNodeCost(node);
    }
    
    private List<Vector2> CreatePathFromNode(Node node)
    {
        var tempPath = new List<Vector2>();
        Node tempNode = node;
        while (tempNode.parent != null)
        {
            tempPath.Add(tempNode.position + _tileAnchor);
            tempNode = tempNode.parent;
        }

        tempPath.Reverse();
        return tempPath;
    }
    
    private void AddNodeToChecked(Node node)
    {
        _uncheckedNodes.Remove(node);
        if(!_checkedNodes.ContainsKey(node.position))
            _checkedNodes.Add(node.position, node);
    }
    
    private void UpdateNodeCost(Node newNode)
    {
        if(!_checkedNodes.ContainsKey(newNode.position)) 
            return;
        
        Node chekedNode = _checkedNodes[newNode.position];
        if (chekedNode.fullPathCost > newNode.fullPathCost)
        {
            _checkedNodes[newNode.position] = newNode;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if(!_drawGizmo) return;

        DrawGizmosCheckedNodes();
        DrawGizmosPath();

    }

    private void DrawGizmosCheckedNodes()
    {
        if (_checkedNodes != null)
        {
            foreach (var node in _checkedNodes)
            {
                Gizmos.color = _checkedNodeColor;
                Gizmos.DrawSphere(node.Value.position + _tileAnchor, 0.1f);
            }
        }
    }

    private void DrawGizmosPath()
    {
        if (_path == null) return;

        for (int i = 0; i < _path.Count - 1; i++)
        {
            Gizmos.color = _pathColor;
            Gizmos.DrawLine(_path[i], _path[i+1]);
        }
    }
}