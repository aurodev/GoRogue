﻿using System.Collections.Generic;
using System;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// Represents an arbitrarily-shaped area of a map. Stores and provides access to a list of each
    /// unique position considered connected.
    /// </summary>
    public class MapArea
    {
        private List<Coord> _positions;

        private int left, top, bottom, right;

        private readonly HashSet<Coord> positionsSet;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MapArea()
        {
            left = int.MaxValue;
            top = int.MaxValue;

            right = 0;
            bottom = 0;

            _positions = new List<Coord>();
            positionsSet = new HashSet<Coord>();
        }

        /// <summary>
        /// Smallest possible rectangle that encompasses every position in the area.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                if (right < left)
                    return Rectangle.EMPTY;

                return new Rectangle(left, top, right - left + 1, bottom - top + 1);
            }
        }

        /// <summary>
        /// Number of (unique) positions in the currently stored list.
        /// </summary>
        public int Count { get { return _positions.Count; } }

        /// <summary>
        /// List of all (unique) positions in the list.
        /// </summary>
        public IReadOnlyList<Coord> Positions { get { return _positions.AsReadOnly(); } }

        /// <summary>
        /// Gets a MapArea containing exactly those positions in both of the given MapAreas.
        /// </summary>
        /// <param name="area1">First MapArea.</param>
        /// <param name="area2">Second MapArea.</param>
        /// <returns>A MapArea containing exactly those positions in both of the given MapAreas.</returns>
        public static MapArea GetIntersection(MapArea area1, MapArea area2)
        {
            var retVal = new MapArea();

            if (!area1.Bounds.Intersects(area2.Bounds))
                return retVal;

            if (area1.Count > area2.Count)
                Utility.Swap(ref area1, ref area2);

            foreach (var pos in area1.Positions)
                if (area2.Contains(pos))
                    retVal.Add(pos);

            return retVal;
        }

        /// <summary>
        /// Gets a new MapArea containing exactly every position in one or both given map areas.
        /// </summary>
        /// <param name="area1">First MapArea.</param>
        /// <param name="area2">Second MapArea.</param>
        /// <returns>A MapArea containing only those positions in one or both of the given MapAreas.</returns>
        public static MapArea GetUnion(MapArea area1, MapArea area2)
        {
            var retVal = new MapArea();

            retVal.Add(area1);
            retVal.Add(area2);

            return retVal;
        }

        /// <summary>
        /// Adds the given position to the list of points within the area if it is not already in the
        /// list, or does nothing otherwise. Because the class uses a hash set internally to
        /// determine what points have already been added, this is an average case O(1) operation.
        /// </summary>
        /// <param name="position">The position to add.</param>
        public void Add(Coord position)
        {
            if (positionsSet.Add(position))
            {
                _positions.Add(position);

                // Update bounds
                if (position.X > right) right = position.X;
                if (position.X < left) left = position.X;
                if (position.Y > bottom) bottom = position.Y;
                if (position.Y < top) top = position.Y;
            }
        }

        /// <summary>
        /// Adds the given position to the list of points within the area if it is not already in the
        /// list, or does nothing otherwise. Because the class uses a hash set internally to
        /// determine what points have already been added, this is an average case O(1) operation.
        /// </summary>
        /// <param name="x">X-coordinate of the position to add.</param>
        /// <param name="y">Y-coordinate of the position to add.</param>
        public void Add(int x, int y) => Add(Coord.Get(x, y));

        /// <summary>
        /// Adds all coordinates in the given map area to this one.
        /// </summary>
        /// <param name="area">Area containing positions to add.</param>
        public void Add(MapArea area)
        {
            foreach (var pos in area.Positions)
                Add(pos);
        }

        /// <summary>
        /// Determines whether or not the given position is considered within the area or not.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the specified position is within the area, false otherwise.</returns>
        public bool Contains(Coord position)
        {
            return positionsSet.Contains(position);
        }

        /// <summary>
        /// Determines whether or not the given position is considered within the area or not.
        /// </summary>
        /// <param name="x">X-coordinate of the position to check.</param>
        /// <param name="y">Y-coordinate of the position to check.</param>
        /// <returns>True if the specified position is within the area, false otherwise.</returns>
        public bool Contains(int x, int y)
        {
            return positionsSet.Contains(Coord.Get(x, y));
        }

        /// <summary>
        /// Returns whether or not the given MapArea is completely contained within the current one.
        /// </summary>
        /// <param name="area">MapArea to check.</param>
        /// <returns>
        /// True if the given MapArea is completely contained within the current one, false otherwise.
        /// </returns>
        public bool Contains(MapArea area)
        {
            if (!Bounds.Contains(area.Bounds))
                return false;

            foreach (var pos in area.Positions)
                if (!Contains(pos))
                    return false;

            return true;
        }

        /// <summary>
        /// Returns whether or not the given map area intersects the current one. If you intend to
        /// determine/use the exact intersection based on this return value, it is best to instead
        /// call the MapArea.GetIntersection, and check the number of positions in the result (0 if
        /// no intersection).
        /// </summary>
        /// <param name="area">The MapArea to check.</param>
        /// <returns>True if the given MapArea intersects the current one, false otherwise.</returns>
        public bool Intersects(MapArea area)
        {
            if (!area.Bounds.Intersects(Bounds))
                return false;

            if (Count <= area.Count)
            {
                foreach (var pos in Positions)
                    if (area.Contains(pos))
                        return true;

                return false;
            }

            foreach (var pos in area.Positions)
                if (Contains(pos))
                    return true;

            return false;
        }

        /// <summary>
        /// Same as operator==.  Returns false of obj is not a MapArea.
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if the object given is a MapArea and is equal (contains the same points), false otherwise.</returns>
        public override bool Equals(object obj)
        {
            var area = obj as MapArea;
            if (area == null) return false;

            return this == area;
        }

        /// <summary>
        /// Compares for equality.  Returns true if the two MapAreas are the same reference, or
        /// if they contain exactly the same points.
        /// </summary>
        /// <param name="lhs">First MapArea to compare.</param>
        /// <param name="rhs">Second MapArea to compare.</param>
        /// <returns>True if the MapAreas contain exactly the same points, false otherwise.</returns>
        public static bool operator==(MapArea lhs, MapArea rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;

            if (lhs.Count != rhs.Count)
                return false;

            foreach (var pos in lhs.Positions)
                if (!rhs.Contains(pos))
                    return false;

            return true;
        }

        /// <summary>
        /// Inequality comparison -- true if the two areas do NOT contain exactly the same points.
        /// </summary>
        /// <param name="lhs">First MapArea to compare.</param>
        /// <param name="rhs">Second MapArea to compare.</param>
        /// <returns>True if the MapAreas do NOT contain exactly the same points, false otherwise.</returns>
        public static bool operator !=(MapArea lhs, MapArea rhs) => !(lhs == rhs);

        /// <summary>
        /// Returns hash of the underlying set.
        /// </summary>
        /// <returns>Hash code for the underlying set.</returns>
        public override int GetHashCode() => positionsSet.GetHashCode();

        /// <summary>
        /// Returns the string of each position in the MapArea, in a square-bracket enclosed list, eg.
        /// [(1, 2), (3, 4), (5, 6)].
        /// </summary>
        /// <returns>A string representation of those coordinates in the MapArea.</returns>
        public override string ToString() => _positions.ExtendToString();
    }
}