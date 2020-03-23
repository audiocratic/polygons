using System;
using System.Collections.Generic;
using System.Linq;

namespace polygons
{
    class Program
    {
        static void Main(string[] args)
        {
            var points = new List<Point>();

            points.Add(
                new Point(
                    -(decimal)88.238705992698669,
                    (decimal)40.107903178857882
                )
            );

            points.Add(
                new Point(
                    -(decimal)88.23868989944458,
                    (decimal)40.106528699670136
                )
            );

            points.Add(
                new Point(
                    -(decimal)88.241554498672485,
                    (decimal)40.106504081879272
                )
            );

            points.Add(
                new Point(
                    -(decimal)88.241570591926575,
                    (decimal)40.107894973094382                  
                )
            );


            var lines = new List<LineSegment>();

            for(int i = 0; i < points.Count - 1; i++)
            {
                var endPoints = points
                    .Skip(i)
                    .Take(2)
                    .ToList();

                lines.Add(
                    new LineSegment(
                        endPoints.First(),
                        endPoints.Last()
                    )
                );
            }

            lines.Add(
                new LineSegment(
                    points.Last(),
                    points.First()
                )
            );

            var polygon = new SimplePolygon();

            lines.ForEach(l => {
                polygon.AddLine(l);
                Console.WriteLine(polygon.IsValid);
            });

            Console.WriteLine(polygon.ContainsPoint(
                new Point(-(decimal)88.240399, (decimal)40.107953)
            ));

            Console.ReadKey();
        }
    }

    public class Point : IEquatable<Point>
    {
        public Point(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }

        private decimal _X { get; set; }

        public decimal X 
        {
            get => _X;
            set => _X = value;
        }

        private decimal _Y { get; set; }

        public decimal Y
        {
            get => _Y;
            set => _Y = value;
        }

        public bool Equals(Point point)
            => Math.Round(X, 5) == Math.Round(point.X, 5)
                && Math.Round(Y, 5) == Math.Round(point.Y, 5);
    }

    public class LineSegment : IEquatable<LineSegment>
    {
        public Point A
            => _A;

        public Point B
            => _B;

        private Point _A { get; set; }

        private Point _B { get; set ;}

        private Point LeftPoint
        {
            get
            {
                Point point = null;

                if(!IsSlopeUndefined)
                {
                    point = A.X < B.X ? A : B;
                }

                return point;
            }
        }

        private Point RightPoint
        {
            get
            {
                Point point = null;

                if(!IsSlopeUndefined)
                {
                    point = A.X > B.X ? A : B;
                }

                return point;
            }
        }

        private Point TopPoint
        {
            get
            {
                Point point = null;

                if(Slope != 0)
                {
                    point = A.Y > B.Y ? A : B;
                }

                return point;
            }
        }       

        private Point BottomPoint
        {
            get
            {
                Point point = null;

                if(Slope != 0)
                {
                    point = A.Y < B.Y ? A : B;
                }

                return point;
            }
        }

        private decimal Top
            => Math.Max(A.Y, B.Y);

        private decimal Bottom
            => Math.Min(A.Y, B.Y);

        private decimal Left
            => Math.Min(A.X, B.X);

        private decimal Right
            => Math.Max(A.X, B.X);

        public bool IsSlopeUndefined 
            => A.X == B.X;
        
        private decimal? Slope
        {
            get
            {
                decimal? slope = null;

                if(A.Y == B.Y) slope = 0;

                if(!IsSlopeUndefined)
                {
                    slope = (RightPoint.Y - LeftPoint.Y) / (RightPoint.X - LeftPoint.X);
                }

                return slope;
            }
        }

        public decimal? YIntercept
        {
            get
            {
                decimal? yIntercept = null;

                if(!IsSlopeUndefined && Slope != 0)
                {
                    yIntercept = A.Y - Slope * A.X;
                }
                else if(Slope == 0)
                {
                    yIntercept = A.Y;
                }

                return yIntercept;
            }
        }

        public LineSegment(Point a, Point b)
        {
            if(a == null || b == null)
                throw new NullReferenceException("Neither point can be null.");

            if(a.Equals(b))
                throw new InvalidOperationException("Points can not be equal.");
            
            _A = a;
            _B = b;
        }
    
        public bool IsColinear(Point point)
        {
            var isColinear = false;

            if(Slope == 0)
            {
                isColinear = Math.Round(point.Y, 5) == Math.Round(A.Y, 5) 
                    && point.X >= LeftPoint.X
                    && point.X <= RightPoint.X;
            }
            else if(IsSlopeUndefined)
            {
                isColinear = Math.Round(point.X, 5) == Math.Round(A.X, 5)
                    && point.Y >= BottomPoint.Y
                    && point.Y <= TopPoint.Y;
            }
            else
            {
                isColinear = point.X >= LeftPoint.X
                    && point.X <= RightPoint.X
                    && point.Y >= BottomPoint.Y
                    && point.Y <= TopPoint.Y;

                if(isColinear)
                {
                    var calculatedYPointOnLine = Slope.Value * point.X + YIntercept.Value;
                    isColinear = Math.Round(point.Y, 5) == Math.Round(calculatedYPointOnLine, 5);
                }
            }

            return isColinear;
        }

        public bool IsParallelTo(LineSegment line)
            => (IsSlopeUndefined && line.IsSlopeUndefined)
                || (Slope == line.Slope);

        public bool Intersects(LineSegment line)
        {
            var intersects = false;

            if(!IsParallelTo(line))
            {
                var point = GetLineIntersectionPoint(line);

                intersects = IsColinear(point)
                    && line.IsColinear(point);
            }

            return intersects;
        }

        public bool IntersectsAtEndpoint(LineSegment line)
            => A.Equals(line.A) 
                || A.Equals(line.B) 
                || B.Equals(line.A) 
                || A.Equals(line.B);
            

        public Point GetLineIntersectionPoint(LineSegment line)
        {
            Point point = null;

            // Solving in order of simplicity (easiest to hardest)
            // 1. One line has undefined slope and the other has slope of 0
            // 2. One line has undefined slope and the other has slope not 0
            // 3. One line has slope of 0 and the other has slope not 0
            // 4. Both lines have slope not 0

            if(!IsParallelTo(line))
            {
                
                // Check if either line has undefined slope
                LineSegment line1 = (IsSlopeUndefined ? this : null)
                    ?? (line.IsSlopeUndefined ? line : null);
                
                LineSegment line2;

                // If there is a line that had undefined slope
                if(line1 != null)
                {
                    // Set line 2 to be the other line
                    line2 = IsSlopeUndefined ? line : this;

                    // Scenario 1
                    if(line2.Slope == 0)
                    {
                        point = new Point(line1.A.X, line2.A.Y);
                    }
                    // Scenario 2
                    else
                    {
                        point = new Point(
                            line1.A.X, 
                            line2.Slope.Value * line1.A.X + line2.YIntercept.Value);
                    }
                }
                else
                {
                    line1 = this;
                    line2 = line;
                    
                    var slopeDifferential = line1.Slope.Value + line2.Slope.Value * -1;

                    var yInterceptDifferential = line2.YIntercept.Value + line1.YIntercept.Value * -1;

                    var x = slopeDifferential == 0 ?
                        yInterceptDifferential :
                        yInterceptDifferential / slopeDifferential;

                    // y depends on if either line is horizontal
                    var horizontalLine =
                        (line1.Slope == 0 ? line1 : null)
                            ?? (line2.Slope == 0 ? line2 : null);

                    var y =
                        horizontalLine != null ?
                        horizontalLine.YIntercept.Value :
                        line1.Slope.Value * x + line1.YIntercept.Value;

                    // This is odd -- cleaning up because apparently -0 is a thing
                    x = x == -0 ? 0 : x;
                    y = y == -0 ? 0 : y;

                    point = new Point(x, y);
                }
            }

            return point;
        }

        public bool Equals(LineSegment line)
            => (A.Equals(line.A) && B.Equals(line.B))
                || (A.Equals(line.B) && B.Equals(line.A));
    }

    public class SimplePolygon
    {
        private List<LineSegment> Lines { get; set; }
        private bool _IsValid { get; set; }
        public bool IsValid
            => _IsValid;

        
        public bool ContainsPoint(Point point)
        {
            var doesContain = false;

            // Check if point is on the border
            if(IsOnBorder(point)) doesContain = true;

            // If not on border, check if inside borders
            if(!doesContain)
            {
                if(IsInsideBorders(point)) doesContain = true;
            }

            return doesContain; 
        }

        private int CountIntersections(LineSegment line)
            => Lines.Where(l => l.Intersects(line)).Count();

        /// <summary>
        /// Checks if a point falls on a corner of the shape (an endpoint of a line segment)
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool IsOnCorner(Point point)
            => Lines
                .Where(l => l.A.Equals(point) || l.B.Equals(point))
                .Count() > 0;

        /// <summary>
        /// Checks if a point falls anywhere on the border of the shape, including corners
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool IsOnBorder(Point point)
            => !IsOnCorner(point)
                && Lines
                    .Where(l => l.IsColinear(point))
                    .Count() > 0;

        private bool IsInsideBorders(Point point)
        {
            var testLine = new LineSegment(
                point,
                new Point(int.MaxValue, point.Y)
            );

            var countIntersections = CountIntersections(testLine);

            return countIntersections % 2 > 0;
        }

        /// <summary>
        /// Adds line after a check for uniqueness of the line and
        /// connections between lines
        /// </summary>
        /// <param name="line"></param>
        public void AddLine(LineSegment line)
        {
            Lines = Lines ?? new List<LineSegment>();

            Lines.ForEach(l => {
                if(l.Equals(line))
                    throw new Exception("All lines must be unique.");
            });

            var countSharedEndpointsA = Lines
                .Where(l => l.A.Equals(line.A) || l.B.Equals(line.A))
                .Count();

            var countSharedEndpointsB = Lines
                .Where(l => l.A.Equals(line.B) || l.B.Equals(line.B))
                .Count();

            if(countSharedEndpointsA > 1 || countSharedEndpointsB > 1)
                throw new Exception("Only two lines can share the same endpoint.");
            
            Lines.Add(line);

            Validate();
        }

        private void Validate()
        {
            // 1.   One line should connect to the next with the final line connecting to the first line
            //      (quickest way I can think to do this is process of elimination following the connections)
            // 2.   No lines should intersect at a point other than their start/end points

            _IsValid = Lines.Count > 2
                && !DoAnyLinesIntersect()
                && AreConnectionsValid();
        }

        private bool AreConnectionsValid()
        {
            bool areValid = true;

            var connectedLines = new List<LineSegment>();
            var remainingLines = new List<LineSegment>();
            remainingLines.AddRange(Lines);

            var line = Lines.FirstOrDefault();

            if(line == null) areValid = false;

            if(areValid)
            {
                connectedLines.Add(line);
                remainingLines.Remove(line);

                for(int i = 1; i < Lines.Count && areValid; i++)
                {
                    var nextLine = remainingLines
                        .FirstOrDefault(l => line.IntersectsAtEndpoint(l));

                    if(nextLine == null)
                    {
                        areValid = false;
                    }
                    else
                    {
                        line = nextLine;
                        connectedLines.Add(line);
                        remainingLines.Remove(line);
                    }
                }

                // Ensure the shape is complete
                if(areValid
                    && !connectedLines
                        .First()
                        .IntersectsAtEndpoint(connectedLines.Last()))
                        areValid = false;
            }
            
            return areValid;
        }

        private bool DoAnyLinesIntersect()
        {
            bool doLinesIntersect = false;
            
            var linesToCheck = new List<LineSegment>();
            linesToCheck.AddRange(Lines);

            Lines.ForEach(l => {
                linesToCheck.ForEach(l2 => {
                    if(!l.IntersectsAtEndpoint(l2) && l.Intersects(l2))
                        doLinesIntersect = true;
                });

                linesToCheck.Remove(l);
            });     

            return doLinesIntersect;
        }
    }

}
