using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Plugins;
using Tekla.Structures.Solid;
using static Tekla.Structures.Model.Position;
using Identifier = Tekla.Structures.Identifier;


namespace Apex_haunch_connection

{
    public class PluginData
    {
        #region Fields

        [StructuresField("PlateThickness1")]
        public double PlateThickness1;

        [StructuresField("PlateThickness2")]
        public double PlateThickness2;

        [StructuresField("PlateHightTop")]
        public double PlateHightTop;

        [StructuresField("PlateHightMid")]
        public double PlateHightMid;

        [StructuresField("PlateHightBottom")]
        public double PlateHightBottom;

        [StructuresField("PlateWidth")]
        public double PlateWidth;

        [StructuresField("FlagBolt")]
        public int FlagBolt;

        [StructuresField("FlagWasher1")]
        public int FlagWasher1;

        [StructuresField("FlagWasher2")]
        public int FlagWasher2;

        [StructuresField("FlagWasher3")]
        public int FlagWasher3;

        [StructuresField("FlagNut1")]
        public int FlagNut1;

        [StructuresField("FlagNut2")]
        public int FlagNut2;

        [StructuresField("BoltSize")]
        public int BoltSize;

        [StructuresField("BoltStandard")]
        public int BoltStandard;

        [StructuresField("BoltToletance")]
        public double BoltToletance;

        [StructuresField("BoltThreadMat")]
        public int BoltThreadMat;

        [StructuresField("BA1yCount")]
        public int BA1yCount;

        [StructuresField("BA1yText")]
        public string BA1yText;

        [StructuresField("BA1xCount")]
        public int BA1xCount;

        [StructuresField("BA1xText")]
        public string BA1xText;

        [StructuresField("BA1OffsetX")]
        public double BA1OffsetX;

        [StructuresField("BA1OffsetY")]
        public double BA1OffsetY;

        [StructuresField("HaunchWebThickness")]
        public double HaunchWebThickness;

        [StructuresField("FlangeThickness")]
        public double FlangeThickness;

        [StructuresField("HaunchWidth")]
        public double HaunchWidth;

        [StructuresField("Material")]
        public string Material;

        #endregion
    }

    [Plugin("Apex_haunch_connection")]
    [PluginUserInterface("Apex_haunch_connection.MainForm")]
    public class Apex_haunch_connection : PluginBase
    {
        Model myModel = new Model();
        #region Fields
        private Model _Model;
        private PluginData _Data;

        private double _PlateThickness1;
        private double _PlateThickness2;
        private double _PlateHightTop;
        private double _PlateHightMid;
        private double _PlateHightBottom;
        private double _PlateWidth;
        private int _BoltSize;
        private int _BoltStandard;
        private double _BoltToletance;
        private int _BoltThreadMat;
        private int _BA1yCount;
        private string _BA1yText;
        private int _BA1xCount;
        private string _BA1xText;
        private int _FlagBolt;
        private int _FlagWasher1;
        private int _FlagWasher2;
        private int _FlagWasher3;
        private int _FlagNut1;
        private int _FlagNut2;
        private double _BA1OffsetX;
        private double _BA1OffsetY;

        private double _HaunchWebThickness;
        private double _FlangeThickness;
        private double _HaunchWidth;

        private string _Material;


        private List<string> _BoltStandardEnum = new List<string>
        {
            "8.8XOX",
            "4.6CSK",
            "4.6CUP",
            "4.6FIRE",
            "4.6XOX",
            "8.8CSK",
            "8.8CUP",
            "8.8FIRE",
            "8.8XOX",
            "E.B",
            "HSFG-XOX",
            "UNDEFINED_BOLT",
            "UNDEFINED_STUD"
        };

        private List<double> _BoltSizeEnum = new List<double>
        {
            10.00,
            12.00,
            16.00,
            20.00,
            24.00,
            30.00
        };

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

                Beam beam1 = myModel.SelectModelObject(Input[0].GetInput() as Identifier) as Beam;
                Beam beam2 = myModel.SelectModelObject(Input[1].GetInput() as Identifier) as Beam;
               
               
                Point origin1 = beam1.EndPoint;
                var girtCoord = beam1.GetCoordinateSystem();
                girtCoord.Origin = origin1;
                //girtCoord.AxisX = girtCoord.AxisX *- 1;
                
                TransformationPlane currentTransformation = myModel.GetWorkPlaneHandler().GetCurrentTransformationPlane();
                var newWorkPlane = new TransformationPlane(girtCoord);
               // workPlaneHandler.SetCurrentTransformationPlane(newWorkPlane);
                myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(newWorkPlane);
                GeometricPlane geometricPlane = Fitparts(beam1 as Part, beam2 as Part, _PlateThickness1, _PlateThickness2);
                ArrayList plates = Plates(beam1, beam2, _PlateHightTop, _PlateHightMid, _PlateHightBottom, _PlateWidth, _PlateThickness1, _PlateThickness2, geometricPlane);
                boltArray(plates, beam1, beam2);
                Hunch(beam1, beam2, plates, _PlateHightBottom, _HaunchWebThickness, _FlangeThickness, _HaunchWidth);
                //workPlaneHandler.SetCurrentTransformationPlane(currentTransformation);
                myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentTransformation);
            }
            catch (Exception Exc)
            {
                MessageBox.Show(Exc.ToString());
            }

            return true;
        }
        #endregion

        #region Private methods

        private void GetValuesFromDialog()
        {
            _PlateThickness1 = Data.PlateThickness1;
            _PlateThickness2 = Data.PlateThickness2;
            _PlateHightTop = Data.PlateHightTop;
            _PlateHightMid = Data.PlateHightMid;
            _PlateHightBottom = Data.PlateHightBottom;
            _PlateWidth = Data.PlateWidth;
            _FlagBolt = Data.FlagBolt;
            _FlagWasher1 = Data.FlagWasher1;
            _FlagWasher2 = Data.FlagWasher2;
            _FlagWasher3 = Data.FlagWasher3;
            _FlagNut1 = Data.FlagNut1;
            _FlagNut2 = Data.FlagNut2;
            _BoltSize = Data.BoltSize;
            _BoltStandard = Data.BoltStandard;
            _BoltToletance = Data.BoltToletance;
            _BoltThreadMat = Data.BoltThreadMat;
            _BA1yCount = Data.BA1yCount;
            _BA1yText = Data.BA1yText;
            _BA1xCount = Data.BA1xCount;
            _BA1xText = Data.BA1xText;
            _BA1OffsetX = Data.BA1OffsetX;
            _BA1OffsetY = Data.BA1OffsetY;

            _FlangeThickness = Data.FlangeThickness;
            _HaunchWebThickness = Data.HaunchWebThickness;
            _HaunchWidth = Data.HaunchWidth;

            _Material = Data.Material;

            if (IsDefaultValue(_PlateThickness1))
                _PlateThickness1 = 10;
            if (IsDefaultValue(_PlateThickness2))
                _PlateThickness2 = 10;
            if (IsDefaultValue(_PlateHightTop))
                _PlateHightTop = 10;
            if (IsDefaultValue(_PlateHightMid))
                _PlateHightMid = 300;
            if (IsDefaultValue(_PlateHightBottom))
                _PlateHightBottom = 10;
            if (IsDefaultValue(_PlateWidth))
                _PlateWidth = 200;
            if (IsDefaultValue(_BoltSize))
            {
                _BoltSize = 0;
            }
            if (IsDefaultValue(_BoltStandard))
            {
                _BoltStandard = 0;
            }
            if (IsDefaultValue(_BoltThreadMat))
            {
                _BoltThreadMat = 0;
            }
            if (IsDefaultValue(_BoltToletance))
            {
                _BoltToletance = 3;
            }
            if (IsDefaultValue(_FlagBolt))
            {
                _FlagBolt = 0;
            }
            if (IsDefaultValue(_FlagWasher1))
            {
                _FlagWasher1 = 0;
            }
            if (IsDefaultValue(_FlagWasher2))
            {
                _FlagWasher2 = 1;
            }
            if (IsDefaultValue(_FlagWasher3))
            {
                _FlagWasher3 = 1;
            }
            if (IsDefaultValue(_FlagNut1))
            {
                _FlagNut1 = 0;
            }
            if (IsDefaultValue(_FlagNut2))
            {
                _FlagNut2 = 1;
            }
            if (IsDefaultValue(_BA1xCount))
            {
                _BA1xCount = 3;
            }
            if (IsDefaultValue(_BA1xText))
            {
                _BA1xText = "50";
            }
            if (IsDefaultValue(_BA1yCount))
            {
                _BA1yCount = 2;
            }
            if (IsDefaultValue(_BA1yText))
            {
                _BA1yText = "60";
            }

            if (IsDefaultValue(_BA1OffsetX))
            { _BA1OffsetX = 0; }

            if (IsDefaultValue(_BA1OffsetY))
                { _BA1OffsetY = 0; }
    
            if (IsDefaultValue(_FlangeThickness))
                _FlangeThickness = 10; 

            if (IsDefaultValue(_HaunchWebThickness))
            { _HaunchWebThickness = 10; }

            if (IsDefaultValue(_HaunchWidth))
                _HaunchWidth = 150;
            if (IsDefaultValue(_Material))
                _Material = "IS2062";
        }

        private GeometricPlane Fitparts(Part part1, Part part2, double thickness1, double thickness2)
        {
            List<Face_> part1Faces = get_faces(part1);
            List<Face_> part2Faces = get_faces(part2);
            ArrayList part1_centerLine = part1.GetCenterLine(false);
            Point part1mid = MidPoint(part1_centerLine[0] as Point, part1_centerLine[1] as Point);
            ArrayList part2_centerLine = part2.GetCenterLine(false);
            Point part2mid = MidPoint(part2_centerLine[0] as Point, part2_centerLine[1] as Point);

            LineSegment intersectLineSegment = Intersection.LineToLine(new Line(part1_centerLine[0] as Point, part1_centerLine[1] as Point), new Line(part2_centerLine[0] as Point, part2_centerLine[1] as Point));
            Point intersectionMidPoint = MidPoint(intersectLineSegment.StartPoint, intersectLineSegment.EndPoint);
            double d1 = Distance.PointToPoint(intersectionMidPoint, part1mid),
                d2 = Distance.PointToPoint(intersectionMidPoint, part2mid);
            Point p1, p2;
            if (d1 > d2)
            {
                p2 = part2mid;

                p1 = FindPointOnLine(intersectionMidPoint, part1mid, d2);


            }
            else
            {
                p1 = part1mid;
                p2 = FindPointOnLine(intersectionMidPoint, part2mid, d1);

            }
            GeometricPlane newplain = CreatePlaneFromThreePoints(intersectionMidPoint, p1, p2);
            Point mid = MidPoint(p1, p2);
            Point point3 = mid + newplain.GetNormal() * 50;
            GeometricPlane fittingPlain = CreatePlaneFromThreePoints(intersectionMidPoint, mid, point3);
            Point point1 = intersectionMidPoint + thickness1 * fittingPlain.GetNormal();
            Point point2 = intersectionMidPoint - thickness2 * fittingPlain.GetNormal();
            var plaine = ConvertGeometricPlaneToPlane(fittingPlain);

            GeometricPlane planeA1 = ConvertFaceToGeometricPlane(part1Faces[5].Face),
              planeA2 = ConvertFaceToGeometricPlane(part1Faces[11].Face),
              planeB1 = ConvertFaceToGeometricPlane(part2Faces[5].Face),
              planeB2 = ConvertFaceToGeometricPlane(part2Faces[11].Face);


            Line line1 = Intersection.PlaneToPlane(planeA1, planeB1),
            line2 = Intersection.PlaneToPlane(planeA2, planeB1),
            line3 = Intersection.PlaneToPlane(planeA1, planeB2),
            line4 = Intersection.PlaneToPlane(planeA2, planeB2);
            Line holdLine = null;
            double d = -1;
            foreach (var line in new List<Line> { line1,line2,line3,line4} )
            {
                if ( d < Distance.PointToLine(mid,line) )
                {
                    d = Distance.PointToLine(mid,line);
                    holdLine = line;
                }
            }
            Point holdPoint1 = Intersection.LineToPlane(holdLine, newplain),
                holdPoint2 = Projection.PointToPlane(holdPoint1,fittingPlain);
            Line l1; 
            if (Distance.PointToPoint(holdPoint1, part1mid) < Distance.PointToPoint(holdPoint2, part1mid))            
                l1 = new Line(holdPoint2, holdPoint1);
            else
                l1 = new Line(holdPoint1, holdPoint2);
            Vector vector = new Vector(l1.Direction);
            point1 = point1 + vector.GetNormal() * Distance.PointToPoint(holdPoint1, holdPoint2);
            point2 = point2 + vector.GetNormal() * Distance.PointToPoint(holdPoint1, holdPoint2);

            Fitting fitting1 = new Fitting();
            fitting1.Plane.AxisX = plaine.AxisX;
            fitting1.Plane.AxisY = plaine.AxisY;
            fitting1.Father = part1;
            

            Fitting fitting2 = new Fitting();
            fitting2.Plane.AxisX = plaine.AxisX;
            fitting2.Plane.AxisY = plaine.AxisY;
            fitting2.Father = part2;
            if (Distance.PointToPoint(point1, part1mid) < Distance.PointToPoint(point2, part1mid))
            {
                fitting1.Plane.Origin = point1;
                fitting2.Plane.Origin = point2;
            }
            else
            {
                fitting1.Plane.Origin = point2;
                fitting2.Plane.Origin = point1;
            }
            fitting1.Insert();
            fitting2.Insert();
            
            return new GeometricPlane(holdPoint1,fittingPlain.GetNormal());
        }
        private ArrayList Plates(Part part1, Part part2, double topHight, double middleHight, double bottomHight, double width, double thickness1, double thickness2, GeometricPlane geometricPlane)
        {
            ArrayList part1_centerLine = part1.GetCenterLine(false);
            ArrayList part2_centerLine = part2.GetCenterLine(false);
            List<Face_> part1Faces = get_faces(part1),
                part2Faces = get_faces(part2);
            Point p1 = Intersection.LineToPlane(new Line(part1_centerLine[0] as Point, part1_centerLine[1] as Point), geometricPlane),
                p2 = Intersection.LineToPlane(new Line(part2_centerLine[0] as Point, part2_centerLine[1] as Point), geometricPlane);
            Point intersectionMidPoint = MidPoint(p1, p2);
            GeometricPlane gp = new GeometricPlane(intersectionMidPoint, part1Faces[2].Vector);
            Line line = Intersection.PlaneToPlane(gp, geometricPlane);
            Point refference = Projection.PointToLine(MidPoint(part1_centerLine[0] as Point, part1_centerLine[1] as Point), line);

            GeometricPlane planeA1 = ConvertFaceToGeometricPlane(part1Faces[5].Face),
               planeA2 = ConvertFaceToGeometricPlane(part1Faces[11].Face),
               planeB1 = ConvertFaceToGeometricPlane(part2Faces[5].Face),
               planeB2 = ConvertFaceToGeometricPlane(part2Faces[11].Face),
               g1 = ConvertFaceToGeometricPlane(part1Faces[0].Face),
               g2 = ConvertFaceToGeometricPlane(part1Faces[10].Face);


            Line line1 = Intersection.PlaneToPlane(planeA1, geometricPlane),
            line2 = Intersection.PlaneToPlane(planeA2, geometricPlane),
            line3 = Intersection.PlaneToPlane(planeB1, geometricPlane),
            line4 = Intersection.PlaneToPlane(planeB2, geometricPlane);
            Point top = GetClosestPointOnLineSegment(refference, Intersection.LineToPlane(line1, g1), Intersection.LineToPlane(line1, g2));
            double distance = Distance.PointToLine(refference, line1);

            foreach (Line l in new List<Line> { line2, line3, line4 })
            {


                if (Distance.PointToLine(refference, l) > distance)
                {
                    top = GetClosestPointOnLineSegment(refference, Intersection.LineToPlane(l, g1), Intersection.LineToPlane(l, g2));
                    distance = Distance.PointToLine(refference, l);
                }
            }

            Vector vector = geometricPlane.GetNormal();

            Point startPoint = FindPointOnLine(top, refference, topHight * -1);
            double totalBottomdistance = middleHight + bottomHight;
            Point endPoint = FindPointOnLine(intersectionMidPoint, refference, totalBottomdistance);
            Beam beam1 = new Beam();
            beam1.StartPoint = startPoint;
            beam1.EndPoint = endPoint;
            beam1.Profile.ProfileString = "PLT" + thickness1 + "*" + width;
            beam1.Position.Depth = Position.DepthEnum.MIDDLE;
            beam1.Position.Plane = Position.PlaneEnum.RIGHT;
            beam1.Position.Rotation = Position.RotationEnum.TOP;
            beam1.Material.MaterialString = _Material;
            beam1.Class = "1";
            beam1.Insert();
            Beam beam2 = new Beam();
            beam2.StartPoint = startPoint;
            beam2.EndPoint = endPoint;
            beam2.Profile.ProfileString = "PLT" + thickness2 + "*" + width;
            beam2.Position.Depth = Position.DepthEnum.MIDDLE;
            beam2.Position.Plane = Position.PlaneEnum.LEFT;
            beam2.Position.Rotation = Position.RotationEnum.TOP;
            beam2.Material.MaterialString = _Material;
            beam2.Class = "1";
            beam2.Insert();
            return new ArrayList { beam1, beam2 };



        }
        private void boltArray(ArrayList parts, Part beam1, Part beam2)
        {

            BoltArray bA = new BoltArray();
            bA.PartToBeBolted = parts[0] as Beam;
            bA.PartToBoltTo = parts[1] as Beam;

            ArrayList beam1Centerline = beam1.GetCenterLine(false),
                beam2Centerline = beam2.GetCenterLine(false);
            LineSegment intersection_CenterLine = Intersection.LineToLine(new Line(beam1Centerline[0] as Point, beam1Centerline[1] as Point), new Line(beam2Centerline[0] as Point, beam2Centerline[1] as Point));

            List<Face_> face_s = get_faces(parts[0] as Beam);
            List<Face_> cp_faces = face_s.OrderByDescending(fa => CalculateFaceArea(fa)).ToList();

            bA.BoltSize = _BoltSizeEnum[_BoltSize];
            bA.Tolerance = _BoltToletance;
            bA.BoltStandard = _BoltStandardEnum[_BoltStandard];
            bA.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
            
            bA.ThreadInMaterial = (_BoltThreadMat == 0) ? BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES : BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_NO;

            bA.Position.Depth = Position.DepthEnum.MIDDLE;
            bA.Position.Plane = Position.PlaneEnum.MIDDLE;
            Vector vector = cp_faces[0].Vector;
            bA.Position.Rotation = (vector.X == 1 || vector.X == -1 || vector.Z == 1) ? RotationEnum.FRONT : RotationEnum.TOP;

            bA.Bolt = (_FlagBolt == 0) ? true : false;
            bA.Washer1 = (_FlagWasher1 == 0) ? true : false;
            bA.Washer2 = (_FlagWasher2 == 0) ? true : false;
            bA.Washer3 = (_FlagWasher3 == 0) ? true : false;
            bA.Nut1 = (_FlagNut1 == 0) ? true : false;
            bA.Nut2 = (_FlagNut2 == 0) ? true : false;


            double total = 0;
            List<double> doubles = InputConverter(_BA1xText);
            bool flag = false;
            double hold = 0;

            if (doubles == null)
                bA.AddBoltDistX(0);
            if (_BA1xCount > 0 && doubles != null)
            {
                if (doubles[0] != 0)
                    bA.AddBoltDistX(0);
                if (doubles.Count == 1)
                    flag = true;
                for (int i = 0; i < _BA1xCount - 1; i++)
                {
                    if (i == doubles.Count - 1)
                    {
                        hold = doubles[i];
                    }
                    if (i >= doubles.Count)
                    {
                        bA.AddBoltDistX(hold);
                        total += hold;
                    }
                    else
                    {
                        bA.AddBoltDistX((flag) ? doubles[0] : doubles[i]);
                        total += (flag) ? doubles[0] : doubles[i];

                    }
                }
            }
            bA.StartPointOffset.Dx = 0;
            if (doubles != null)
                doubles.Clear();
            doubles = InputConverter(_BA1yText);

            if (doubles == null)
                bA.AddBoltDistY(0);
            if (_BA1yCount > 0 && doubles != null)
            {
                if (doubles[0] != 0)
                    bA.AddBoltDistY(0);
                if (doubles.Count == 1)
                    flag = true;
                for (int i = 0; i < _BA1yCount - 1; i++)
                {
                    if (i == doubles.Count - 1)
                    {
                        hold = doubles[i];
                    }
                    if (i >= doubles.Count)
                    {
                        bA.AddBoltDistY(hold);

                    }
                    else
                    {
                        bA.AddBoltDistY((flag) ? doubles[0] : doubles[i]);
                    }
                }
            }

            bA.StartPointOffset.Dz = _BA1OffsetY;
            bA.EndPointOffset.Dz = _BA1OffsetY;

            GeometricPlane gp1 = ConvertFaceToGeometricPlane(cp_faces[0].Face),
               gp2 = ConvertFaceToGeometricPlane(cp_faces[1].Face);
            GeometricPlane geometricPlane = new GeometricPlane();
            if (Distance.PointToPlane(intersection_CenterLine.StartPoint, gp1) > Distance.PointToPlane(intersection_CenterLine.StartPoint, gp2))
                geometricPlane = gp1;
            else
                geometricPlane = gp2;
            Point mid = MidPoint(intersection_CenterLine.StartPoint, intersection_CenterLine.EndPoint);
            Beam beam = parts[0] as Beam;
            Point point1 = FindPointOnLine(mid, beam.StartPoint, total /  2 + _BA1OffsetX);
            bA.FirstPosition = Projection.PointToPlane(point1, geometricPlane);
            bA.SecondPosition = Projection.PointToPlane(beam.EndPoint, geometricPlane);
            bA.Insert();
        }
        private void Hunch(Part part1, Part part2, ArrayList plates, double bottom_length, double webThickness, double flangeThickness, double width)
        {
            ArrayList part1_centerLine = part1.GetCenterLine(false);
            ArrayList part2_centerLine = part2.GetCenterLine(false);
            List<Face_> part1Faces = get_faces(part1),
               part2Faces = get_faces(part2);
            Beam plate1 = plates[0] as Beam,
                plate2 = plates[1] as Beam;

            List<Face_> face_s = get_faces(plates[0] as Beam);
            List<Face_> plate1_faces = face_s.OrderByDescending(fa => CalculateFaceArea(fa)).ToList();
            face_s = get_faces(plates[1] as Beam);
            List<Face_> plate2_faces = face_s.OrderByDescending(fa => CalculateFaceArea(fa)).ToList();
            GeometricPlane plate1Closest = null, plate2Closest = null;
            
            GeometricPlane plA1 = ConvertFaceToGeometricPlane(plate1_faces[0].Face),
                plA2 = ConvertFaceToGeometricPlane(plate1_faces[1].Face),
                plB1 = ConvertFaceToGeometricPlane(plate2_faces[0].Face),
                plB2 = ConvertFaceToGeometricPlane(plate2_faces[1].Face);
            double d = 0;
            foreach (GeometricPlane gp in new List<GeometricPlane> { plA1, plA2, plB1, plB2 })
            {
                if (d < Distance.PointToPlane(MidPoint(part1_centerLine[0] as Point, part1_centerLine[1] as Point), gp))
                {
                    d = Distance.PointToPlane(MidPoint(part1_centerLine[0] as Point, part1_centerLine[1] as Point), gp);
                    plate2Closest = gp;
                }
            }
            d=0;
            foreach (GeometricPlane gp in new List<GeometricPlane> { plA1, plA2, plB1, plB2 })
            {
                if (d < Distance.PointToPlane(MidPoint(part2_centerLine[0] as Point, part2_centerLine[1] as Point), gp))
                {
                    d = Distance.PointToPlane(MidPoint(part2_centerLine[0] as Point, part2_centerLine[1] as Point), gp);
                    plate1Closest = gp;
                }
            }
            
           
           

            GeometricPlane part1FaceColsest = null, part2FaceClosest = null;
            if (Distance.PointToPlane(plate1.EndPoint, ConvertFaceToGeometricPlane(part1Faces[5].Face)) < Distance.PointToPlane(plate1.EndPoint, ConvertFaceToGeometricPlane(part1Faces[11].Face)))
                part1FaceColsest = ConvertFaceToGeometricPlane(part1Faces[5].Face);
            else
                part1FaceColsest = ConvertFaceToGeometricPlane(part1Faces[11].Face);

            if (Distance.PointToPlane(plate1.EndPoint, ConvertFaceToGeometricPlane(part2Faces[5].Face)) < Distance.PointToPlane(plate1.EndPoint, ConvertFaceToGeometricPlane(part2Faces[11].Face)))
                part2FaceClosest = ConvertFaceToGeometricPlane(part2Faces[5].Face);
            else
                part2FaceClosest = ConvertFaceToGeometricPlane(part2Faces[11].Face);

            Point holdStart = Projection.PointToPlane(plate1.StartPoint,plate1Closest ), holdEnd = Projection.PointToPlane(plate1.EndPoint, plate1Closest);
            Point pA1 = Intersection.LineToPlane(new Line(holdStart, holdEnd), part1FaceColsest);
            Point pA2 = FindPointOnLine(holdEnd, holdStart, bottom_length + flangeThickness);
            Line holdLine = new Line(pA2, plate1Closest.GetNormal());
            Point pA3 = Intersection.LineToPlane(holdLine, part1FaceColsest);

            ContourPlate cp1 = new ContourPlate();
            ArrayList countourPoints = new ArrayList();

            foreach (Point point in new List<Point> { pA1, pA2, pA3 })
            {
                ContourPoint contourPoint = new ContourPoint(point, new Chamfer(10, 10, Chamfer.ChamferTypeEnum.CHAMFER_LINE));
                countourPoints.Add(contourPoint);
            }

            cp1.Contour.ContourPoints = countourPoints;

            cp1.Profile.ProfileString = "PLT" + webThickness;

            cp1.Material.MaterialString = "IS2062";
            cp1.Class = "4";
            cp1.Position.Depth = Position.DepthEnum.MIDDLE;
            cp1.Insert();

            holdStart = Projection.PointToPlane(plate2.StartPoint,plate2Closest); holdEnd = Projection.PointToPlane(plate2.EndPoint, plate2Closest);
            Point pB1 = Intersection.LineToPlane(new Line(holdStart, holdEnd), part2FaceClosest);
            Point pB2 = FindPointOnLine(holdEnd, holdStart, bottom_length + flangeThickness);
            holdLine = new Line(pA2, plate1Closest.GetNormal());
            Point pB3 = Intersection.LineToPlane(holdLine, part2FaceClosest);

            ContourPlate cp2 = new ContourPlate();
            ArrayList countourPoints1 = new ArrayList();

            foreach (Point point in new List<Point> { pB1, pB2, pB3 })
            {
                ContourPoint contourPoint = new ContourPoint(point, new Chamfer(10, 10, Chamfer.ChamferTypeEnum.CHAMFER_LINE));
                countourPoints1.Add(contourPoint);
            }

            cp2.Contour.ContourPoints = countourPoints1;

            cp2.Profile.ProfileString = "PLT" + webThickness;

            cp2.Material.MaterialString = "IS2062";
            cp2.Class = "4";
            cp2.Position.Depth = Position.DepthEnum.MIDDLE;
            cp2.Insert();

            Beam flange1 = new Beam();
            flange1.StartPoint = pA2;
            flange1.EndPoint = pA3;
            Vector vector1 = new Line(pA2, pA3).Direction;
            vector1.Normalize();
            flange1.Profile.ProfileString = "PLT" + flangeThickness + "*" + width;
            flange1.Position.Depth = Position.DepthEnum.MIDDLE;
            flange1.Position.Plane =(vector1.X > 0 && vector1.Y > 0)? PlaneEnum.RIGHT : PlaneEnum.LEFT;
            flange1.Position.Rotation = Position.RotationEnum.TOP;
            flange1.Material.MaterialString = _Material;
            flange1.Class = "1";
            flange1.Insert();

            Beam flange2 = new Beam();
            flange2.StartPoint = pB2;
            flange2.EndPoint = pB3;
            Vector vector2 = new Line(pB2, pB3).Direction;
            vector2.Normalize();
            flange2.Profile.ProfileString = "PLT" + flangeThickness + "*" + width;
            flange2.Position.Depth = Position.DepthEnum.MIDDLE;
            flange2.Position.Plane = (vector1.X > 0 && vector1.Y > 0) ? PlaneEnum.LEFT : PlaneEnum.RIGHT;
            flange2.Position.Rotation = Position.RotationEnum.TOP;
            flange2.Material.MaterialString = _Material;
            flange2.Class = "1";
            flange2.Insert();

            WeldArray(cp1, new ArrayList { part1, plates[0], flange1 });
            WeldArray(cp2, new ArrayList { part2, plates[1], flange2 });
        }
        private void WeldArray(Part part1 , ArrayList arrayList)
        {
            foreach (Part part2 in arrayList)
            {
                Weld Weld = new Weld();
                Weld.MainObject = part1;
                Weld.SecondaryObject = part2;
                Weld.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                Weld.SizeAbove = 5;
                Weld.SizeBelow = 5;
                
                Weld.LengthAbove = 12;
                Weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                Weld.Insert();

                
            }
            Weld Weld1 = new Weld();
            Weld1.MainObject = arrayList[0] as Part;
            Weld1.SecondaryObject = arrayList[1] as Part;
            Weld1.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld1.SizeAbove = 5;
            Weld1.SizeBelow = 5;

            Weld1.LengthAbove = 12;
            Weld1.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld1.Insert();

            Weld1 = new Weld();
            Weld1.MainObject = arrayList[2] as Part;
            Weld1.SecondaryObject = arrayList[1] as Part;
            Weld1.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld1.SizeAbove = 5;
            Weld1.SizeBelow = 5;

            Weld1.LengthAbove = 12;
            Weld1.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld1.Insert();

            Weld1 = new Weld();
            Weld1.MainObject = arrayList[0] as Part;
            Weld1.SecondaryObject = arrayList[2] as Part;
            Weld1.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld1.SizeAbove = 5;
            Weld1.SizeBelow = 5;

            Weld1.LengthAbove = 12;
            Weld1.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld1.Insert();

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
        private void GetFaceAxes(Face face, out Vector xAxis, out Vector yAxis)
        {
            Vector normalVector;
            // Get the loop vertices of the face to extract points
            ArrayList vertices = Get_Points(face);

            if (vertices == null || vertices.Count < 3)
            {
                throw new ArgumentException("The face does not have enough vertices to define axes.");
            }

            // Select three distinct points to define the plane and axes
            Point point1 = vertices[0] as Point;
            Point point2 = vertices[1] as Point;
            Point point3 = vertices[2] as Point;

            // Define the X-axis vector as the vector between point1 and point2
            xAxis = new Vector(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            xAxis.Normalize();

            // Define another vector on the face
            Vector vector2 = new Vector(point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z);

            // Calculate the normal vector (cross product of xAxis and vector2)
            normalVector = Vector.Cross(xAxis, vector2);
            normalVector.Normalize();

            // Define the Y-axis vector as the cross product of the normal vector and X-axis vector
            yAxis = Vector.Cross(normalVector, xAxis);
            yAxis.Normalize();
        }
        private ArrayList Get_Points(Face face)
        {
            ArrayList points = new ArrayList();
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
            return points;
        }
        public static double DistanceBetweenParallelLines(Line line1, Line line2)
        {
            // Get the direction vector of the first line (parallel to the second line)
            Vector directionLine1 = new Vector(line1.Origin.X - line1.Origin.X, line1.Origin.Y - line1.Origin.Y, line1.Origin.Z - line1.Origin.Z);

            // Vector between a point on line1 and a point on line2
            Vector connectingVector = new Vector(line2.Origin.X - line1.Origin.X, line2.Origin.Y - line1.Origin.Y, line2.Origin.Z - line1.Origin.Z);

            // Find the cross product of the direction vector and the connecting vector
            Vector crossProduct = directionLine1.Cross(connectingVector);

            // The magnitude of the cross product divided by the magnitude of the direction vector gives the shortest distance
            double distance = crossProduct.GetLength() / directionLine1.GetLength();

            return distance;
        }
        private static Point GetClosestPointOnLineSegment(Point point, Point lineStart, Point lineEnd)
        {
            // Vector from line start to the point
            Vector startToPoint = new Vector(point - lineStart);

            // Direction vector of the line segment
            Vector lineDirection = new Vector(lineEnd - lineStart);
            double lineLengthSquared = lineDirection.Dot(lineDirection);

            // Project the point onto the line segment
            double t = startToPoint.Dot(lineDirection) / lineLengthSquared;

            // Clamp t to the range [0, 1] to keep the projection within the segment
            t = Math.Max(0, Math.Min(1, t));

            // Calculate the closest point on the line segment
            return new Point(
                lineStart.X + t * lineDirection.X,
                lineStart.Y + t * lineDirection.Y,
                lineStart.Z + t * lineDirection.Z
            );
        }

        public static Point FindPointOnLine(Point startPoint, Point secondPoint, double distance)
        {
            if (distance == 0)
                return startPoint;
            // Step 1: Calculate the direction vector from startPoint to secondPoint
            Vector direction = new Vector(
                secondPoint.X - startPoint.X,
                secondPoint.Y - startPoint.Y,
                secondPoint.Z - startPoint.Z
            );

            // Step 2: Normalize the direction vector
            direction.Normalize();

            // Step 3: Scale the direction vector by the distance
            Vector scaledVector = new Vector(
                direction.X * distance,
                direction.Y * distance,
                direction.Z * distance
            );

            // Step 4: Calculate the new point by adding the scaled vector to the start point
            Point newPoint = new Point(
                startPoint.X + scaledVector.X,
                startPoint.Y + scaledVector.Y,
                startPoint.Z + scaledVector.Z
            );

            return newPoint;
        }
        private List<double> InputConverter(string input)
        {
            if (input == "")
                return null;
            string[] hold = input.Split(' ');
            List<double> output = new List<double>();
            foreach (string s in hold)
            {
                if (s.Contains('*'))
                {
                    string[] strings = s.Split('*');
                    for (int i = 0; i < int.Parse(strings[0]); i++)
                    {
                        output.Add(double.Parse(strings[1]));
                    }
                }
                else
                {
                    double d;
                    if (double.TryParse(s, out d))
                        output.Add(d);
                }
            }
            return output;
        }
        private double CalculateFaceArea(Face_ face)
        {
            ArrayList facePoints = Get_Points(face.Face); // Assuming this method gets the list of points of the face

            if (facePoints.Count < 3)
                return 0.0; // A face must have at least 3 points to form a polygon

            double totalArea = 0.0;
            Point basePoint = facePoints[0] as Point;

            // Iterate through the face points and form triangles with the base point
            for (int i = 1; i < facePoints.Count - 1; i++)
            {
                Point point1 = facePoints[i] as Point;
                Point point2 = facePoints[i + 1] as Point;

                // Calculate the area of the triangle formed by basePoint, point1, and point2
                totalArea += CalculateTriangleArea(basePoint, point1, point2);
            }

            return totalArea;
        }
        private static double CalculateTriangleArea(Point p1, Point p2, Point p3)
        {
            // Create vectors representing two sides of the triangle
            Vector v1 = new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            Vector v2 = new Vector(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);

            // The area of the triangle is half the magnitude of the cross product of the vectors
            Vector crossProduct = v1.Cross(v2);
            double area = 0.5 * crossProduct.GetLength();
            return area;
        }
        
        public static Tekla.Structures.Model. Plane ConvertGeometricPlaneToPlane(GeometricPlane geometricPlane)
        {
            // Extract the point on the plane
            Point origin = geometricPlane.Origin;

            // Extract the normal vector of the plane
            Vector normal = geometricPlane.Normal;

            // Create a new Plane using the origin and normal vector
            Tekla.Structures.Model.Plane plane = new Tekla.Structures.Model.Plane();
            plane.Origin = origin;
            plane.AxisX = normal.Cross(new Vector(0, 0, 1)); // X-axis direction (perpendicular to Z-axis)
            plane.AxisY = normal.Cross(plane.AxisX);         // Y-axis direction
            plane.AxisX.Normalize();
            plane.AxisY.Normalize();

            return plane;
        }
        #endregion
    }
}
