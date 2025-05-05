using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum BookingStatus
{
    CheckIn = 1,
    CheckOut = 2
}

class HotelCapacity
{
    public static void Main()
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
        // var result = CheckCapacityIfExtensionAllowed(maxCapacity, guests);

        Console.WriteLine(result ? "True" : "False");
    }
    
    /// <summary>
    /// В условии не сказано про то, может ли один и тот же гость встречаться в списке дважды.
    /// Если может, то он занимает тот же номер, поэтому нужно учесть продление брониварония,
    /// как это делается в методе CheckCapacityIfExtensionAllowed.
    /// </summary>
    private static bool CheckCapacity(int maxCapacity, List<Guest> guests)
    {
        var bookings = new List<(string Date, BookingStatus Status)>();
        foreach (var guest in guests)
        {
            bookings.Add((guest.CheckIn, BookingStatus.CheckIn));
            bookings.Add((guest.CheckOut, BookingStatus.CheckOut));
        }

        bookings.Sort((first, second) =>
        {
            var dateComparison = string.Compare(first.Date, second.Date, StringComparison.Ordinal);
            return dateComparison != 0
                ? dateComparison
                : first.Status.CompareTo(second.Status);
        });

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
    
    /// <summary>
    /// Проверяет, возможно ли разместить всех гостей, если допускать продлевание бронирования.
    /// Учитывает случаи, когда в списке гостей встречается один и тот же гость, даты бронирований которого пересекаются.
    /// </summary>
    private static bool CheckCapacityIfExtensionAllowed(int maxCapacity, List<Guest> guests)
    {
        var bookings = new List<(string Name, string Date, BookingStatus Status)>();
        var currentBookingsCountByName = new Dictionary<string, int>();
        foreach (var guest in guests)
        {
            bookings.Add((guest.Name, guest.CheckIn, BookingStatus.CheckIn));
            bookings.Add((guest.Name, guest.CheckOut, BookingStatus.CheckOut));
            currentBookingsCountByName.TryAdd(guest.Name, 0);
        }

        bookings.Sort((first, second) =>
        {
            var dateComparison = string.Compare(first.Date, second.Date, StringComparison.Ordinal);
            return dateComparison != 0
                ? dateComparison
                : first.Status.CompareTo(second.Status);
        });

        return CheckCapacityIfExtensionAllowed(maxCapacity, bookings, currentBookingsCountByName);
    }

    private static bool CheckCapacityIfExtensionAllowed(int maxCapacity, 
        List<(string Name, string Date, BookingStatus Status)> bookings, 
        Dictionary<string, int> currentBookingsCountByName)
    {
        var currentOccupiedRoomsCount = 0;
        foreach (var booking in bookings)
        {
            switch (booking.Status)
            {
                case BookingStatus.CheckIn:
                    if (currentBookingsCountByName[booking.Name] == 0)
                        currentOccupiedRoomsCount++;
                    currentBookingsCountByName[booking.Name]++;
                    break;
                case BookingStatus.CheckOut:
                    currentBookingsCountByName[booking.Name]--;
                    if (currentBookingsCountByName[booking.Name] == 0)
                        currentOccupiedRoomsCount--;
                    break;
            }
            
            if (currentOccupiedRoomsCount > maxCapacity)
                return false;
        }
        
        return true;
    }

    private class Guest
    {
        public string Name { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
    }


    private static Guest ParseGuest(string json)
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