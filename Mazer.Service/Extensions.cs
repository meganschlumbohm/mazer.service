namespace Mazer.Service
{
    internal static class Extensions
    {
        internal static void FlipTravelOrientation(this RoomModel room)
        {
            room.TOrientation = room.TOrientation == TravelOrientation.Horizontal ? TravelOrientation.Vertical : TravelOrientation.Horizontal;
        }

        internal static void FlipTravelDirection(this RoomModel room)
        {
           room.TDirection = room.TDirection == TravelDirection.Forward ? TravelDirection.Backward : TravelDirection.Forward;
        }

        internal static MirrorDirection TranslateMirrorDirection(this string direction) =>
            _ = direction == "L" ? MirrorDirection.Left : MirrorDirection.Right;

        internal static MirrorReflection TranslateMirrorReflection(this string reflection) =>
            _ = reflection == "L" ? MirrorReflection.Left : MirrorReflection.Right;

        internal static TravelOrientation TranslateTravelOrientation(this string orientation) =>
            _ = orientation == "H" ? TravelOrientation.Horizontal : TravelOrientation.Vertical;
    }
}
