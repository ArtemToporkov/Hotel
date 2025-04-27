using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum BookingStatus
{
    CheckIn,
    CheckOut
}

class HotelCapacity
{
    static bool CheckCapacity(int maxCapacity, List<Guest> guests)
    {
        var bookings = new List<(string Date, BookingStatus Status)>();
        foreach (var guest in guests)
        {
            bookings.Add((guest.CheckIn, BookingStatus.CheckIn));
            bookings.Add((guest.CheckOut, BookingStatus.CheckOut));
        }

        bookings.Sort((first, second)
            => string.Compare(first.Date, second.Date, StringComparison.Ordinal));

        var currentOccupiedRoomsCount = 0;
        foreach (var booking in bookings)
        {
            currentOccupiedRoomsCount = booking.Status is BookingStatus.CheckIn
                ? currentOccupiedRoomsCount + 1
                : currentOccupiedRoomsCount - 1;
            if (currentOccupiedRoomsCount > maxCapacity)
                return false;
        }
        
        return true;
    }


    class Guest
    {
        public string Name { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
    }


    static void Main()
    {
        var maxCapacity = int.Parse(Console.ReadLine()!);
        var n = int.Parse(Console.ReadLine()!);


        var guests = new List<Guest>();


        for (var i = 0; i < n; i++)
        {
            var line = Console.ReadLine()!;
            var guest = ParseGuest(line);
            guests.Add(guest);
        }


        var result = CheckCapacity(maxCapacity, guests);


        Console.WriteLine(result ? "True" : "False");
    }


    static Guest ParseGuest(string json)
    {
        var guest = new Guest();
        
        var nameMatch = Regex.Match(json, "\"name\"\\s*:\\s*\"([^\"]+)\"");
        if (nameMatch.Success)
            guest.Name = nameMatch.Groups[1].Value;
        
        var checkInMatch = Regex.Match(json, "\"check-in\"\\s*:\\s*\"([^\"]+)\"");
        if (checkInMatch.Success)
            guest.CheckIn = checkInMatch.Groups[1].Value;
        
        var checkOutMatch = Regex.Match(json, "\"check-out\"\\s*:\\s*\"([^\"]+)\"");
        if (checkOutMatch.Success)
            guest.CheckOut = checkOutMatch.Groups[1].Value;


        return guest;
    }
}