using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;

using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Plugins;
using Tekla.Structures.Solid;

namespace Apex_haunch_connection

{
    public class PluginData
    {
        #region Fields
        
        [StructuresField("PlateThickness1")]
        public double PlateThickness1;

        [StructuresField("PlateThickness2")]
        public double PlateThickness2;

        [StructuresField("RebarGrade")]
        public string RebarGrade;

        [StructuresField("RebarBendingRadius")]
        public string RebarBendingRadius;

        [StructuresField("RebarClass")]
        public int RebarClass;

        [StructuresField("RebarSpacing")]
        public double RebarSpacing;
        
        #endregion
    }

    [Plugin("Apex_haunch_connection")]
    [PluginUserInterface("Apex_haunch_connection.MainForm")]
    public class Apex_haunch_connection : PluginBase
    {
        #region Fields
        private Model _Model;
        private PluginData _Data;
        //
        // Define variables for the field values.
        //
        /* Some examples:
        private string _RebarName = string.Empty;
        private string _RebarSize = string.Empty;
        private string _RebarGrade = string.Empty;
        private ArrayList _RebarBendingRadius = new ArrayList();
        private int _RebarClass = new int();
        private double _RebarSpacing;
        */
        #endregion

        #region Properties
        private Model Model
        {
            get { return this._Model; }
            set { this._Model = value; }
        }

        private PluginData Data
        {
            get { return this._Data; }
            set { this._Data = value; }
        }
        #endregion

        #region Constructor
        public Apex_haunch_connection(PluginData data)
        {
            Model = new Model();
            Data = data;
        }
        #endregion

        #region Overrides
        public override List<InputDefinition> DefineInput()
        {
            List<InputDefinition> PointList = new List<InputDefinition>();
            Picker Picker = new Picker();
            var part = Picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick Primary object");
            var partno1 = part;
            PointList.Add(new InputDefinition(partno1.Identifier));
            part = Picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick Secondary object");
            var partno2 = part;
            PointList.Add(new InputDefinition(partno2.Identifier));
            return PointList;
        }

        public override bool Run(List<InputDefinition> Input)
        {
            try
            {
                GetValuesFromDialog();

                //
                // This is an example for selecting two points; change this to suit your needs.
                //
                ArrayList Points = (ArrayList)Input[0].GetInput();
                Point StartPoint = Points[0] as Point;
                Point EndPoint = Points[1] as Point;

                //
                // Write your code here; better yet, create private methods and call them from here.
                //
            }
            catch (Exception Exc)
            {
                MessageBox.Show(Exc.ToString());
            }

            return true;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Gets the values from the dialog and sets the default values if needed
        /// </summary>
        private void GetValuesFromDialog()
        {
            /* Some examples:
            _RebarName = Data.RebarName;
            _RebarSize = Data.RebarSize;
            _RebarGrade = Data.RebarGrade;

            char[] Parameters = { ' ' };
            string[] Radiuses = Data.RebarBendingRadius.Split(Parameters, StringSplitOptions.RemoveEmptyEntries);

            foreach (string Item in Radiuses)
                _RebarBendingRadius.Add(Convert.ToDouble(Item));

            _RebarClass = Data.RebarClass;
            _RebarSpacing = Data.RebarSpacing;

            if (IsDefaultValue(_RebarName))
                _RebarName = "REBAR";
            if (IsDefaultValue(_RebarSize))
                _RebarSize = "12";
            if (IsDefaultValue(_RebarGrade))
                _RebarGrade = "Undefined";
            if (_RebarBendingRadius.Count < 1)
                _RebarBendingRadius.Add(30.00);
            if (IsDefaultValue(_RebarClass) || _RebarClass <= 0)
                _RebarClass = 99;
            if (IsDefaultValue(_RebarSpacing) || _RebarSpacing <= 0)
                _RebarSpacing = 200.0;
            */
        }

        private void Fitparts(Part part1, Part part2, double gap1, double gap2,double thickness1 , double thickness2)
        {
            List<Face_> part1_Face = get_faces(part1);
            List<Face_> part2_Face = get_faces(part2);
            ArrayList part1_centerLine = part1.GetCenterLine(false);
            ArrayList part2_centerLine = part2.GetCenterLine(false);
            LineSegment intersectLineSegment = Intersection.LineToLine(new Line(part1_centerLine[0] as Point, part1_centerLine[1] as Point), new Line(part2_centerLine[0] as Point, part2_centerLine[1] as Point));
            Point intersectionMidPoint = MidPoint(intersectLineSegment.StartPoint, intersectLineSegment.EndPoint);
            GeometricPlane planeA1 = ConvertFaceToGeometricPlane(part1_Face[5].Face),
                planeA2 = ConvertFaceToGeometricPlane(part1_Face[11].Face),
                planeB1 = ConvertFaceToGeometricPlane(part2_Face[5].Face),
                planeB2 = ConvertFaceToGeometricPlane(part2_Face[11].Face);

            Line line1 = Intersection.PlaneToPlane(planeA1, planeB1),
            line2 = Intersection.PlaneToPlane(planeA1, planeB2),
            line3 = Intersection.PlaneToPlane(planeA2, planeB1),
            line4 = Intersection.PlaneToPlane(planeA2, planeB2);
            GeometricPlane geometricPlane;
            Point p1, p2, p3;
            if (Intersection.LineToLine(line1, line4).Length() > Intersection.LineToLine(line2, line3).Length())
            {
                p1 = Intersection.LineToPlane(line1, ConvertFaceToGeometricPlane(part1_Face[0].Face));
                p2 = Intersection.LineToPlane(line1, ConvertFaceToGeometricPlane(part1_Face[10].Face));
                p3 = Intersection.LineToPlane(line4, ConvertFaceToGeometricPlane(part1_Face[0].Face));
                geometricPlane = CreatePlaneFromThreePoints(p1, p2, p3);
            }
            else
            {
                p1 = Intersection.LineToPlane(line2, ConvertFaceToGeometricPlane(part1_Face[0].Face));
                p2 = Intersection.LineToPlane(line2, ConvertFaceToGeometricPlane(part1_Face[10].Face));
                p3 = Intersection.LineToPlane(line3, ConvertFaceToGeometricPlane(part1_Face[0].Face));
                geometricPlane = CreatePlaneFromThreePoints(p1, p2, p3);
            }
            Fitting fitting = new Fitting();
            fitting.Plane = new Tekla.Structures.Model.Plane();
            Vector vector = geometricPlane.Normal;
            
            Point point1 = new Point(p1.X + thickness1 * vector.X, p1.Y + thickness1 * vector.Y, p1.Z + thickness1 * vector.Z);
            fitting.Plane.Origin = point1;
            
            GetFaceAxes(face.Face, out Vector xAxis, out Vector yAxis);
            fitting.Plane.AxisX = xAxis;
            fitting.Plane.AxisY = yAxis;
            fitting.Father = beam2;
            fitting.Insert();
        }

        class Face_
        {
            public Face Face { get; set; }
            public Vector Vector { get; set; }
            public void face_(Face face, Vector vector)
            {
                Face = face;
                Vector = vector;
            }
        }
        private List<Face_> get_faces(Part beam)
        {

            Solid solid = beam.GetSolid();
            FaceEnumerator faceEnumerator = solid.GetFaceEnumerator();
            List<Face_> faces = new List<Face_>();
            while (faceEnumerator.MoveNext())
            {

                Face face = faceEnumerator.Current as Face;
                Vector vector = face.Normal;
                faces.Add(new Face_ { Face = face, Vector = vector });

            }

            return faces;
        }
        private Point MidPoint(Point point, Point point1)
        { 
            Point mid = new Point((point.X + point1.X) / 2, (point.Y + point1.Y) / 2, (point.Z + point1.Z) / 2);
            return mid;
        }
        private static GeometricPlane ConvertFaceToGeometricPlane(Face face)
        {
            ArrayList points = new ArrayList();
            // Get the edges from the face (since 'Points' is not available)
            LoopEnumerator loopEnumerator = face.GetLoopEnumerator();
            while (loopEnumerator.MoveNext())
            {

                Loop loop = loopEnumerator.Current as Loop;
                VertexEnumerator vertexEnumerator = loop.GetVertexEnumerator();
                while (vertexEnumerator.MoveNext())
                {
                    points.Add(vertexEnumerator.Current);
                }
            }

            Point point1 = points[0] as Point;
            Point point2 = points[1] as Point;
            Point point3 = points[2] as Point;



            if (point1 == null || point2 == null || point3 == null)
            {
                throw new ArgumentException("The face does not have sufficient points to define a plane.");
            }

            // Create vectors from the points
            Vector vector1 = new Vector(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            Vector vector2 = new Vector(point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z);

            // Calculate the normal vector (cross product of the two vectors)
            Vector normalVector = Vector.Cross(vector1, vector2);
            normalVector.Normalize();

            // Create the geometric plane using point1 and the normal vector
            GeometricPlane geometricPlane = new GeometricPlane(point1, normalVector);

            return geometricPlane;
        }
        public static GeometricPlane CreatePlaneFromThreePoints(Point point1, Point point2, Point point3)
        {
            // Calculate two direction vectors on the plane
            Vector vector1 = new Vector(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            Vector vector2 = new Vector(point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z);

            // Calculate the normal vector of the plane by taking the cross product of the two direction vectors
            Vector normalVector = vector1.Cross(vector2);

            // Create and return the geometric plane using the first point and the normal vector
            GeometricPlane plane = new GeometricPlane(point1, normalVector);

            return plane;
        }
        
        #endregion
    }
}
