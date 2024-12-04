using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mazer.Service
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine("Welcome to Laser Mazer.");

            do
            {
                try
                {
                    FindLaserExit();
                }
                catch (Exception ex)
                {
                    // TODO: expect & handle more errors
                    Console.Write("An unexpected error occurred.");
                    Console.WriteLine(ex.Message);
                }

                Console.WriteLine();
                Console.WriteLine("Would you like to test another Maze file? (Y/N)");
            }
            while (Console.ReadLine().ToLower() == "y");
        }

        private static void FindLaserExit()
        {
            Console.WriteLine("Please input Maze file name:");
            var fileName = Console.ReadLine();

            var rooms = BuildRoomModelsFromFile(fileName);
            var entryRoom = rooms.Single(r => r.IsEntry);
            var entryRoomTravelOrientation = entryRoom.TOrientation;

            var currentRoom = rooms.Any(r => r.IsCurrent) ? rooms.Single(r => r.IsCurrent) : rooms.Single(r => r.IsEntry);
            var numRoomsEntered = 1;

            while (!currentRoom.IsExit)
            {
                // if the room has a mirror and we will hit it, flip the travel orientation
                if (currentRoom.HasMirror && currentRoom.WillHitMirror) 
                { 
                    currentRoom.FlipTravelOrientation();
                    // if it's a left facing mirror, flip the travel direction too
                    if (currentRoom.MDirection == MirrorDirection.Left) currentRoom.FlipTravelDirection();
                }

                RoomModel nextRoom = currentRoom;
                switch (currentRoom.CDirection)
                {
                    case (CardinalDirection.North):
                        nextRoom = rooms.SingleOrDefault(r => r.XCoordinate == currentRoom.XCoordinate && r.YCoordinate == currentRoom.YCoordinate + 1);
                        break;
                    case (CardinalDirection.South):
                        nextRoom = rooms.SingleOrDefault(r => r.XCoordinate == currentRoom.XCoordinate && r.YCoordinate == currentRoom.YCoordinate - 1);
                        break;
                    case (CardinalDirection.East):
                        nextRoom = rooms.SingleOrDefault(r => r.XCoordinate == currentRoom.XCoordinate + 1 && r.YCoordinate == currentRoom.YCoordinate);
                        break;
                    case (CardinalDirection.West):
                        nextRoom = rooms.SingleOrDefault(r => r.XCoordinate == currentRoom.XCoordinate - 1 && r.YCoordinate == currentRoom.YCoordinate);
                        break;
                    default:
                        break;
                }

                // if there is not a valid room to travel towards, consider our current room the exit
                if (nextRoom == null)
                {
                    currentRoom.IsExit = true;
                }
                // if there is a valid room to travel towards, set our travel properties and update our current position and room
                else
                {
                    nextRoom.TOrientation = currentRoom.TOrientation;
                    nextRoom.TDirection = currentRoom.TDirection;
                    currentRoom.IsCurrent = false;
                    nextRoom.IsCurrent = true;
                    currentRoom = nextRoom;
                    numRoomsEntered++;
                }

                if (numRoomsEntered > rooms.Count * 4)
                {
                    Console.WriteLine("We have entered an infinite loop. Terminating maze run.");
                    break;
                }
                continue;
            }

            Console.WriteLine($"The dimensions of the board: ({rooms.Max(r => r.XCoordinate) + 1},{rooms.Max(r => r.YCoordinate) + 1})");

            Console.WriteLine($"The start position of the laser: ({entryRoom.XCoordinate},{entryRoom.YCoordinate}) {entryRoomTravelOrientation}");

            var exitRoom = rooms.SingleOrDefault(r => r.IsExit);
            if (exitRoom != null) 
                Console.WriteLine($"The exit point of the laser: ({exitRoom.XCoordinate},{exitRoom.YCoordinate}) {exitRoom.TOrientation}");
        }

        // TODO: parse file in a clean way
        // pulling this into a separate method to pretend it's cleaner
        // should maybe be pulled into it's own service (esp when parsed cleaner)
        private static HashSet<RoomModel> BuildRoomModelsFromFile(string fileName)
        {
            string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;

            var contents = File.ReadAllLines(Path.Combine(projectDirectory, fileName));

            var dimensions = contents[0];
            var dimensionDetail = Regex.Matches(dimensions, @"(\d)+");
            var x = int.Parse(dimensionDetail[0].Value);
            var y = int.Parse(dimensionDetail[1].Value);

            var rooms = new HashSet<RoomModel>();

            // build out blank room grid
            for (var i = 0; i < x; i++)
            {
                for (var j = 0; j < y; j++)
                {
                    rooms.Add(new RoomModel { XCoordinate = i, YCoordinate = j });
                }
            }

            // TODO: dynamically read file based on (-1) rows
            // skip first two lines of maze file
            // skip last three lines of maze file
            for (var i = 2; i < contents.Length - 3; i++)
            {
                var mirror = contents[i];

                var coordinateDetail = Regex.Matches(mirror, @"(\d)+");
                var mirrorXCoord = int.Parse(coordinateDetail[0].Value);
                var mirrorYCoord = int.Parse(coordinateDetail[1].Value);

                var mirrorDetail = Regex.Matches(mirror, "[HLRV]");
                var mDir = mirrorDetail[0].Value.TranslateMirrorDirection();
                var mRef = mirrorDetail.Count > 1 ? mirrorDetail[1].Value.TranslateMirrorReflection() : MirrorReflection.TwoWay;

                var mirrorRoom = rooms.Single(r => r.XCoordinate == mirrorXCoord && r.YCoordinate == mirrorYCoord);
                mirrorRoom.HasMirror = true;
                mirrorRoom.MDirection = mDir;
                mirrorRoom.MReflection = mRef;
            }

            var entry = contents[contents.Length - 2];

            var entryCoordinateDetail = Regex.Matches(entry, @"(\d)+");
            var entryXCoord = int.Parse(entryCoordinateDetail[0].Value);
            var entryYCoord = int.Parse(entryCoordinateDetail[1].Value);

            var travelDetail = Regex.Match(entry, "[HLRV]");
            var entryTravelOrientation = travelDetail.Value.TranslateTravelOrientation();
            var entryRoom = rooms.Single(r => r.XCoordinate == entryXCoord && r.YCoordinate == entryYCoord);

            entryRoom.IsEntry = true;
            entryRoom.TOrientation = entryTravelOrientation;
            entryRoom.TDirection = (entryRoom.TOrientation == TravelOrientation.Vertical && entryYCoord == 0) ||
                (entryRoom.TOrientation == TravelOrientation.Horizontal && entryXCoord == 0) ? TravelDirection.Forward : TravelDirection.Backward;

            return rooms;
        }
    }
}
