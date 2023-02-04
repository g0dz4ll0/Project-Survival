using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EPathfindResult
{
    NoPathdata,
    StartInvalid,
    EndInvalid,
    UnlinkedAreas,
    BudgetExhausted,
    NoPathFound,

    ContextPrepared,
    PathfindingInProgress,

    PathFound
}

public class OpenListComparer : IComparer<PathdataNode>
{
    PathfindingContext LinkedContext;

    public OpenListComparer(PathfindingContext _LinkedContext)
    {
        LinkedContext = _LinkedContext;
    }

    public int Compare(PathdataNode node1, PathdataNode node2)
    {
        float node1F = LinkedContext.GetFCost(node1);
        float node2F = LinkedContext.GetFCost(node2);

        if (node1F < node2F)
            return -1;
        else if (node1F > node2F)
            return 1;

        return node1.UniqueID.CompareTo(node2.UniqueID);
    }
}

public class PathfindingContext 
{
    [System.Flags]
    enum ENodeStatus
    {
        None   = 0x00,
        Open   = 0x01,
        Closed = 0x02
    }

    ENodeStatus[] Statuses;
    float[] GCosts;
    float[] HCosts;
    int[] ParentIDs;

    public int NumIterations { get; private set; } = 0;
    public PathdataNode EndNode { get; private set; } = null;
    public Pathdata LinkedPathdata { get; private set; } = null;

    public System.Func<PathdataNode, PathdataNode, float> CalculateCost;
    public System.Action<List<PathdataNode>, EPathfindResult> OnComplete;

    SortedSet<PathdataNode> OpenList;

    public bool OpenListNotEmpty => OpenList.Count > 0;
    public int IterationBudgetTotal { get; private set; } = 0;
    public int IterationBudgetPerFrame { get; private set; } = 0;

    public PathfindingContext(int iterationBudgetTotal, int iterationBudgetPerFrame, int numNodes,
                              PathdataNode _EndNode, Pathdata _LinkedPathdata,
                              System.Func<PathdataNode, PathdataNode, float> _CalculateCost)
    {
        IterationBudgetTotal = iterationBudgetTotal;
        IterationBudgetPerFrame = iterationBudgetPerFrame;
        EndNode = _EndNode;
        LinkedPathdata = _LinkedPathdata;
        CalculateCost = _CalculateCost;

        Statuses = new ENodeStatus[numNodes];
        GCosts = new float[numNodes];
        HCosts = new float[numNodes];
        ParentIDs = new int[numNodes];

        // inicializa os dados
        for (int index = 0; index < numNodes; ++index)
        {
            ParentIDs[index] = -1;
        }

        OpenList = new SortedSet<PathdataNode>(new OpenListComparer(this));
    }

    public void OpenNode(PathdataNode node, float gCost, float hCost, int parentID)
    {
        Statuses[node.UniqueID] = ENodeStatus.Open;
        GCosts[node.UniqueID] = gCost;
        HCosts[node.UniqueID] = hCost;
        ParentIDs[node.UniqueID] = parentID;

        OpenList.Add(node);
    }

    public void UpdateOpenNode(PathdataNode node, float gCost, int parentID)
    {
        OpenList.Remove(node);

        GCosts[node.UniqueID] = gCost;
        ParentIDs[node.UniqueID] = parentID;

        OpenList.Add(node);
    }

    public void MoveToClosed(PathdataNode node)
    {
        OpenList.Remove(node);
        Statuses[node.UniqueID] = ENodeStatus.Closed;
    }

    public float GetGCost(PathdataNode node)
    {
        return GCosts[node.UniqueID];
    }

    public float GetFCost(PathdataNode node)
    {
        return GCosts[node.UniqueID] + HCosts[node.UniqueID];
    }

    public int GetParentID(PathdataNode node)
    {
        return ParentIDs[node.UniqueID];
    }

    public bool IsNodeOpen(PathdataNode node)
    {
        return Statuses[node.UniqueID] == ENodeStatus.Open;
    }

    public bool IsNodeClosed(PathdataNode node)
    {
        return Statuses[node.UniqueID] == ENodeStatus.Closed;
    }

    public PathdataNode GetBestNode()
    {
        ++NumIterations;

        return OpenList.Min;
    }
}

[System.Serializable]
public class PathfindingConfig
{
    public int IterationBudgetPerCell = 20;
    public int IterationsPerTick = 1;
}

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance { get; private set; } = null;

    [SerializeField] PathfindingConfig AsynchronousConfig;
    [SerializeField] PathfindingConfig SynchronousConfig;

    List<PathfindingContext> AsynchronousPathfinds = new List<PathfindingContext>();

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Econtrado um PathfindingManager duplicado em " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // atualizar todos os pathfinds em progresso
        for (int index = 0; index < AsynchronousPathfinds.Count; ++index)
        {
            var context = AsynchronousPathfinds[index];

            // atualizar o pathfinding
            List<PathdataNode> path;
            var result = PerformPathfind(context, out path);

            // o pathfinding foi feito com sucesso?
            if (result != EPathfindResult.PathfindingInProgress)
            {
                context.OnComplete.Invoke(path, result);
                AsynchronousPathfinds.RemoveAt(0);

                --index;
            }
        }
    }

    ENeighbourFlags[] NeighboursToCheck = new ENeighbourFlags[] {
        ENeighbourFlags.North,
        ENeighbourFlags.NorthEast,
        ENeighbourFlags.East,
        ENeighbourFlags.SouthEast,
        ENeighbourFlags.South,
        ENeighbourFlags.SouthWest,
        ENeighbourFlags.West,
        ENeighbourFlags.NorthWest
    };

    Vector3Int[] NeighbourOffsets = new Vector3Int[] {
        GridHelpers.Step_North,
        GridHelpers.Step_NorthEast,
        GridHelpers.Step_East,
        GridHelpers.Step_SouthEast,
        GridHelpers.Step_South,
        GridHelpers.Step_SouthWest,
        GridHelpers.Step_West,
        GridHelpers.Step_NorthWest,
    };

    public bool OptimisePath(string pathDataUID, List<PathdataNode> path)
    {
        // adquire a pathdata
        var pathdata = PathdataManager.Instance.GetPathdata(pathDataUID);
        if (pathdata == null)
        {
            Debug.LogError("A pathdata não pôde ser adquirida " + pathDataUID);
            return false;
        }

        int currentNode = 0;
        while(currentNode < (path.Count - 1))
        {
            // verifica por nodes mais à frente
            int farthestReachableNode = currentNode + 1;
            for (int nextNode = currentNode + 2; nextNode < (path.Count - 1); ++nextNode)
            {
                if (CanWalkBetween(pathdata, path[currentNode], path[nextNode]))
                    farthestReachableNode = nextNode;
                else
                    break;
            }

            // verifica se há nodes para remover
            int numNodes = farthestReachableNode - currentNode - 1;
            if (numNodes > 0)
                path.RemoveRange(currentNode + 1, numNodes);

            ++currentNode;
        }

        return true;
    }

    public bool CanWalkBetween(string pathDataUID, PathdataNode start, PathdataNode end)
    {
        // adquire a pathdata
        var pathdata = PathdataManager.Instance.GetPathdata(pathDataUID);
        if (pathdata == null)
        {
            Debug.LogError("A pathdata não pôde ser adquirida " + pathDataUID);
            return false;
        }

        return CanWalkBetween(pathdata, start, end);
    }

    // Baseado em Bresenham's Line Drawing Algorithm de: https://iq.opengenus.org/bresenham-line-drawining-algorithm/
    public bool CanWalkBetween(Pathdata pathdata, PathdataNode start, PathdataNode end)
    {
        int deltaX = Mathf.Abs(end.GridPos.x - start.GridPos.x);
        int deltaY = Mathf.Abs(end.GridPos.y - start.GridPos.y);

        // a mover-se maioritariamente em x?
        if (deltaX > deltaY)
        {
            var workingStart = start.GridPos.x < end.GridPos.x ? start : end;
            var workingEnd = start.GridPos.x < end.GridPos.x ? end : start;

            int y = workingStart.GridPos.y;
            int D = 2 * deltaY - deltaX;

            for (int x = workingStart.GridPos.x; x <= workingEnd.GridPos.x; ++x)
            {
                // não é possível andar sobre este ponto?
                if (!pathdata.GetNode(y, x).IsWalkable)
                    return false;

                if (D < 0)
                    D += 2 * deltaY;
                else
                {
                    ++y;
                    D += 2 * deltaY - 2 * deltaX;
                }
            }
        } // a mover-se maioritariamente em y
        else
        {
            var workingStart = start.GridPos.y < end.GridPos.y ? start : end;
            var workingEnd = start.GridPos.y < end.GridPos.y ? end : start;

            int x = workingStart.GridPos.x;
            int D = 2 * deltaX - deltaY;

            for (int y = workingStart.GridPos.y; y <= workingEnd.GridPos.y; ++y)
            {
                // não é possível andar sobre este ponto?
                if (!pathdata.GetNode(y, x).IsWalkable)
                    return false;

                if (D < 0)
                    D += 2 * deltaX;
                else
                {
                    ++x;
                    D += 2 * deltaX - 2 * deltaY;
                }
            }
        }

        return true;
    }

    public EPathfindResult RequestPath_Asynchronous(string pathDataUID, Vector3 startPos, Vector3 endPos, 
                                                    System.Func<PathdataNode, PathdataNode, float> calculateCost,
                                                    System.Action<List<PathdataNode>, EPathfindResult> onComplete)
    {
        // preparar o pathfind
        PathfindingContext context;
        var result = PreparePathfind(AsynchronousConfig, pathDataUID, startPos, endPos, calculateCost, out context);

        // a definição do contexto falhou
        if (result != EPathfindResult.ContextPrepared)
        {
            return result;
        }

        context.OnComplete = onComplete;

        AsynchronousPathfinds.Add(context);

        return EPathfindResult.PathfindingInProgress;
    }

    public EPathfindResult RequestPath_Synchronous(string pathDataUID, Vector3 startPos, Vector3 endPos,
                                                   System.Func<PathdataNode, PathdataNode, float> calculateCost,
                                                   out List<PathdataNode> foundPath)
    {
        // preparar o pathfind
        PathfindingContext context;
        var result = PreparePathfind(SynchronousConfig, pathDataUID, startPos, endPos, calculateCost, out context);

        // a definição do contexto falhou
        if (result != EPathfindResult.ContextPrepared)
        {
            foundPath = null;
            return result;
        }

        return PerformPathfind(context, out foundPath);
    }

    EPathfindResult PreparePathfind(PathfindingConfig configuration,
                                    string pathDataUID, Vector3 startPos, Vector3 endPos,
                                    System.Func<PathdataNode, PathdataNode, float> calculateCost,
                                    out PathfindingContext context)
    {
        context = null;

        // adquire a pathdata
        var pathdata = PathdataManager.Instance.GetPathdata(pathDataUID);
        if (pathdata == null)
        {
            Debug.LogError("A pathdata não pôde ser adquirida " + pathDataUID);
            return EPathfindResult.NoPathdata;
        }

        // adquire o node de começo
        var startNode = pathdata.GetNode(startPos);
        if (startNode == null)
        {
            Debug.LogError("Falha ao adquirir o node em " + startPos);
            return EPathfindResult.StartInvalid;
        }

        // adquire o node do destino
        var endNode = pathdata.GetNode(endPos);
        if (endNode == null)
        {
            Debug.LogError("Falha ao adquirir o node em " + endPos);
            return EPathfindResult.EndInvalid;
        }

        // verifica os ids das áreas
        if (startNode.AreaID != endNode.AreaID || startNode.AreaID < 1 || endNode.AreaID < 1)
        {
            Debug.Log("Não existem caminhos");
            return EPathfindResult.UnlinkedAreas;
        }

        // calcular o budget
        int nodeBudget = Mathf.CeilToInt((endNode.GridPos - startNode.GridPos).magnitude) * configuration.IterationBudgetPerCell;

        // define o contexto
        context = new PathfindingContext(nodeBudget, configuration.IterationsPerTick, pathdata.Nodes.Length, endNode, pathdata, calculateCost);

        // abre o node do começo
        context.OpenNode(startNode, 0f, calculateCost(startNode, endNode), -1);

        return EPathfindResult.ContextPrepared;
    }

    EPathfindResult PerformPathfind(PathfindingContext context, out List<PathdataNode> foundPath)
    {
        foundPath = null;

        // faz um loop enquanto tiver nodes para explorar
        int numIterations = 0;
        while (context.OpenListNotEmpty)
        {
            PathdataNode bestNode = context.GetBestNode();
            ++numIterations;

            // chegou ao destino?
            if (bestNode == context.EndNode)
            {
                foundPath = new List<PathdataNode>();

                while (bestNode != null)
                {
                    foundPath.Insert(0, bestNode);

                    bestNode = context.LinkedPathdata.GetNode(context.GetParentID(bestNode));
                }

                Debug.Log("Caminho encontrado em " + context.NumIterations + " iterações");
                return EPathfindResult.PathFound;
            }

            // chegou ao limite para este frame
            if (context.IterationBudgetPerFrame > 0 && numIterations > context.IterationBudgetPerFrame)
            {
                return EPathfindResult.PathfindingInProgress;
            }

            // chegou ao total do budget?
            if (context.NumIterations >= context.IterationBudgetTotal)
            {
                Debug.LogError("Chegou ao budget de iterações de " + context.IterationBudgetTotal);
                return EPathfindResult.BudgetExhausted;
            }

            // mover para a lista fechada
            context.MoveToClosed(bestNode);

            for (int neighbourIndex = 0; neighbourIndex < NeighboursToCheck.Length; ++neighbourIndex)
            {
                // não existem vizinhos
                if (!bestNode.NeighbourFlags.HasFlag(NeighboursToCheck[neighbourIndex]))
                    continue;

                PathdataNode neighbour = context.LinkedPathdata.GetNode(bestNode.GridPos + NeighbourOffsets[neighbourIndex]);

                // ignora se estiver fechado
                if (context.IsNodeClosed(neighbour))
                    continue;

                // o node está aberto?
                if (context.IsNodeOpen(neighbour))
                {
                    // calcula o custa para chegar ao vizinho
                    float gCost = context.GetGCost(bestNode) + context.CalculateCost(bestNode, neighbour);

                    // foi encontrado outro caminho mais curto?
                    if (gCost < context.GetGCost(neighbour))
                        context.UpdateOpenNode(neighbour, gCost, bestNode.UniqueID);
                }
                else
                {
                    // calcula o custo para chegar ao vizinho
                    float gCost = context.GetGCost(bestNode) + context.CalculateCost(bestNode, neighbour);

                    context.OpenNode(neighbour, gCost, context.CalculateCost(neighbour, context.EndNode), bestNode.UniqueID);
                }
            }
        }

        Debug.LogError("Nenhum caminho encontrado");

        return EPathfindResult.NoPathFound;
    }
}