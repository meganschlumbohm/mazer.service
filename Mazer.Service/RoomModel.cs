namespace Mazer.Service
{
    internal class RoomModel
    {
        public int XCoordinate { get; set; } = 0;
        public int YCoordinate { get; set; } = 0;

        public bool IsCurrent { get; set; } = false;
        public bool IsEntry { get; set; } = false;
        public bool IsExit { get; set; } = false;

        public bool HasMirror { get; set; }
        public MirrorDirection? MDirection { get; set; }
        public MirrorReflection? MReflection { get; set; }
        public TravelDirection? TDirection { get; set; }
        public TravelOrientation? TOrientation { get; set; }
        
        public CardinalDirection? CDirection { get {
                return (TDirection, TOrientation)
                switch
                {
                    (TravelDirection.Forward, TravelOrientation.Vertical) => (CardinalDirection?)CardinalDirection.North,
                    (TravelDirection.Forward, TravelOrientation.Horizontal) => (CardinalDirection?)CardinalDirection.East,
                    (TravelDirection.Backward, TravelOrientation.Vertical) => (CardinalDirection?)CardinalDirection.South,
                    (TravelDirection.Backward, TravelOrientation.Horizontal) => (CardinalDirection?)CardinalDirection.West,
                    _ => (CardinalDirection?)CardinalDirection.North,
                };
            } 
        }

        public bool WillHitMirror { get
            {
                return HasMirror &&
                    (MReflection == MirrorReflection.TwoWay) ||
                    (MReflection == MirrorReflection.Left && MDirection == MirrorDirection.Left && CDirection != CardinalDirection.West && CDirection != CardinalDirection.South) ||
                    (MReflection == MirrorReflection.Left && MDirection == MirrorDirection.Right && CDirection != CardinalDirection.West && CDirection != CardinalDirection.North) ||
                    (MReflection == MirrorReflection.Right && MDirection == MirrorDirection.Left && CDirection != CardinalDirection.East && CDirection != CardinalDirection.North) ||
                    (MReflection == MirrorReflection.Right && MDirection == MirrorDirection.Right && CDirection != CardinalDirection.East && CDirection != CardinalDirection.South);
            }
        }
    }

    internal enum CardinalDirection
    {
        North,
        East,
        South,
        West
    }

    internal enum MirrorDirection
    {
        Left, 
        Right
    }

    internal enum MirrorReflection
    {
        Left,
        Right,
        TwoWay
    }

    internal enum TravelDirection
    {
        Forward,
        Backward
    }

    internal enum TravelOrientation
    {
        Horizontal,
        Vertical
    }
}
