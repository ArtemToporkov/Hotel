using System;
using System.Collections.Generic;
using System.Linq;


class Program
{
    static readonly char[] KeysChar = Enumerable.Range('a', 26).Select(i => (char)i).ToArray();
    static readonly char[] DoorsChar = KeysChar.Select(char.ToUpper).ToArray();
    
    static List<List<char>> GetInput()
    {
        var data = new List<List<char>>();
        string line;
        
        while ((line = Console.ReadLine()) != null && line != "")
            data.Add(line.ToCharArray().ToList());
        
        return data;
    }


    static int Solve(List<List<char>> data)
    {
        throw new NotImplementedException();
    }
    
    static void Main()
    {
        var data = GetInput();
        var result = Solve(data);
        
        if (result == -1)
            Console.WriteLine("No solution found");
        else
            Console.WriteLine(result);
    }
}