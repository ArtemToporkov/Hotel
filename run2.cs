using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


class Program
{
    private static readonly (int Dx, int Dy)[] Moves = { (-1, 0), (1, 0), (0, -1), (0, 1) };
    private const int LimitForDifferentWaysToHitTheCell = 10;

    public static void Main2()
    {
        var data = GetInput();
        var result = Solve(data);
        
        if (result == -1)
            Console.WriteLine("No solution found");
        else
            Console.WriteLine(result);
    }

    private static List<List<char>> GetInput()
    {
        var data = new List<List<char>>();
        string line;

        while ((line = Console.ReadLine()) != null && line != "")
            data.Add(line.ToCharArray().ToList());

        return data;
    }


    private static int Solve(List<List<char>> data)
    {
        var width = data.Count;
        var height = data[0].Count;
        var starts = new (int X, int Y)[4];
        var currentRobotId = 0;
        var requiredKeysMask = 0;
        for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var cell = data[x][y];
                switch (cell)
                {
                    case '@':
                        starts[currentRobotId] = (x, y);
                        currentRobotId++;
                        break;
                    case >= 'a' and <= 'z':
                        requiredKeysMask |= 1 << (cell - 'a');
                        break;
                }
            }

        var graph = GetGraph(data, starts);
        var startPositions = new[] { '0', '1', '2', '3' };
        var cache = new Dictionary<(char, char, char, char, int KeysCollectedMask), int>();
        var result = Dfs(startPositions, 0, requiredKeysMask, cache, graph);
        return result == int.MaxValue ? -1 : result;
    }

    private static Dictionary<char, HashSet<(int Distance, int RequiredKeysMask)>> BfsFromPosition(
        List<List<char>> data, int startX, int startY)
    {
        var width = data.Count;
        var height = data[0].Count;
        var result = new Dictionary<char, HashSet<(int Distance, int RequiredKeysMask)>>();
        var visited = new Dictionary<(int X, int Y), HashSet<int>>
        {
            [(startX, startY)] = new() { 0 }
        };
        var queue = new Queue<(int X, int Y, int Distance, int RequiredKeysMask)>();
        queue.Enqueue((startX, startY, 0, 0));
        while (queue.Any())
        {
            var (x, y, distance, requiredKeysMask) = queue.Dequeue();
            var cell = data[x][y];
            
            if (cell is >= 'a' and <= 'z' && (x != startX || y != startY))
            {
                if (!result.ContainsKey(cell))
                    result[cell] = new();
                result[cell].Add((distance, requiredKeysMask));
            }
            
            for (var i = 0; i < 4; i++)
            {
                var (newX, newY) = (x + Moves[i].Dx, y + Moves[i].Dy);
                if (newX < 0 || newX >= width || newY < 0 || newY >= height
                    || data[newX][newY] == '#' || data[newX][newY] == '@')
                    continue;

                var movedTo = data[newX][newY];
                var newRequiredKeysMask = requiredKeysMask;
                if (movedTo is >= 'A' and <= 'Z')
                    newRequiredKeysMask |= 1 << (movedTo - 'A');

                if (visited.TryGetValue((newX, newY), out var masks) 
                    && (masks.Count >= LimitForDifferentWaysToHitTheCell || !masks.Add(newRequiredKeysMask)))
                    continue;
                queue.Enqueue((newX, newY, distance + 1, newRequiredKeysMask));
                if (!visited.ContainsKey((newX, newY)))
                    visited[(newX, newY)] = new() { newRequiredKeysMask };
            }
        }

        return result;
    }

    private static Dictionary<char, Dictionary<char, HashSet<(int Distance, int RequiredKeysMask)>>> GetGraph(
        List<List<char>> data, (int X, int Y)[] starts)
    {
        var graph = new Dictionary<char, Dictionary<char, HashSet<(int Distance, int RequiredKeysMask)>>>();
        for (var i = 0; i < 4; i++)
        {
            var edges = BfsFromPosition(data, starts[i].X, starts[i].Y);
            graph[(char)('0' + i)] = edges;
        }

        var width = data.Count;
        var height = data[0].Count;
        
        for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var cell = data[x][y];
                if (cell is >= 'a' and <= 'z')
                {
                    if (graph.ContainsKey(cell))
                        continue;
                    var edges = BfsFromPosition(data, x, y);
                    graph[cell] = edges;
                }
            }

        return graph;
    }

    private static int Dfs(char[] positions, int keysCollectedMask, int requiredKeysMask, 
        Dictionary<(char, char, char, char, int KeysCollectedMask), int> cache, 
        Dictionary<char, Dictionary<char, HashSet<(int Distance, int RequiredKeysMask)>>> graph)
    {
        if (keysCollectedMask == requiredKeysMask)
            return 0;

        var stateKey = GetStateKey(positions, keysCollectedMask);
        if (cache.TryGetValue(stateKey, out var dfs))
            return dfs;

        var ans = int.MaxValue;
        for (var i = 0; i < 4; i++)
        {
            var currentPosition = positions[i];
            if (!graph.ContainsKey(currentPosition))
                continue;
            foreach (var (to, edges) in graph[currentPosition])
            {
                foreach (var edge in edges) 
                {
                    var keyMask = 1 << (to - 'a');
                    if ((keysCollectedMask & keyMask) != 0 || 
                        (edge.RequiredKeysMask & keysCollectedMask) != edge.RequiredKeysMask)
                        continue;
                    var previousPosition = positions[i];
                    positions[i] = to;
                    var distance = Dfs(positions, keysCollectedMask | keyMask, requiredKeysMask, cache, graph);
                    positions[i] = previousPosition;
                    if (distance != int.MaxValue)
                        ans = Math.Min(ans, edge.Distance + distance);
                }
            }
        }

        cache[stateKey] = ans;
        return ans;
    }

    private static (char, char, char, char, int KeysCollectedMask) GetStateKey(char[] positions, int keysCollectedMask)
        => (positions[0], positions[1], positions[2], positions[3], keysCollectedMask);
}