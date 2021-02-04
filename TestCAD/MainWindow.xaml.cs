using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using SharpDX.Direct3D11;
using TestCAD.Models;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using MeshGeometry3D = HelixToolkit.SharpDX.Core.MeshGeometry3D;
using OrthographicCamera = HelixToolkit.Wpf.SharpDX.OrthographicCamera;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;

namespace TestCAD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SceneNodeGroupModel3D sceneNodeGroup;
        private PhongMaterialCollection materials = new PhongMaterialCollection();
        private Random rnd = new Random();


        public MainWindow()
        {

            InitializeComponent();

            viewport.Camera = _perspCam;

            sceneNodeGroup = new SceneNodeGroupModel3D();
            viewport.Items.Add(sceneNodeGroup);
            viewport.Items.Add(new AxisPlaneGridModel3D() { UpAxis = Axis.Z });
            viewport.Items.Add(new LineGeometryModel3D() { Name = "grid_XY", Visibility = Visibility.Hidden, Geometry = LineBuilder.GenerateGrid(new Vector3(0, 0, 1), 0, 10), Thickness = 1, Color = Colors.DimGray });
            viewport.Items.Add(new LineGeometryModel3D() { Name = "grid_YZ", Visibility = Visibility.Hidden, Geometry = LineBuilder.GenerateGrid(new Vector3(1, 0, 0), 0, 10), Thickness = 1, Color = Colors.DimGray });
            viewport.Items.Add(new LineGeometryModel3D() { Name = "grid_XZ", Visibility = Visibility.Hidden, Geometry = LineBuilder.GenerateGrid(new Vector3(0, 1, 0), 0, 10), Thickness = 1, Color = Colors.DimGray });


            WindowState = WindowState.Maximized;

            var opacityHelper = new OpacityHelper(viewport);
            dcPanel.DataContext = opacityHelper;
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e) //добавление куба на сцену
        {
            VisualizeFigure(new Box_Model());
        }

        private void Button_Click_AddSphere(object sender, RoutedEventArgs e) //добавление сферы на сцену
        {
            VisualizeFigure(new Sphere_Model());
        }
        private void Button_Click_AddCylinder(object sender, RoutedEventArgs e) //добавление цилиндра на сцену
        {
            VisualizeFigure(new Cylinder_Model());
        }
        private void Button_Click_AddGlass(object sender, RoutedEventArgs e) //добавление чаши на сцену
        {
            VisualizeFigure(new Revolved());
        }
        private void Button_Click_AddExtrusionAngel(object sender, RoutedEventArgs e) //добавление выдавливания на сцену
        {
            VisualizeFigure(new Extrusion_with_angle());
        }
        private void Button_Click_AddExtrusionHole(object sender, RoutedEventArgs e) //добавление выдавливания на сцену
        {
            VisualizeFigure(new Extrusion_with_hole());
        }
        public void VisualizeFigure(BaseModel m)
        {
            // визуализируем исходный контур
            var (points, inxs) = GetContourPointAndInx(m);
            LineGeometry3D lineGeom0 = new() { Positions = points, Indices = inxs, };
            LineGeometryModel3D lines = new() { Geometry = lineGeom0, Color = Colors.Red, Thickness = 3 };
            viewport.Items.Add(lines);

            // визуализируем фигуру
            m.Update();
            var tmp = ToGeometry(m);
            MeshGeometryModel3D model = new() { Geometry = tmp };

            if (m.Error)
            {
                MessageBoxResult result = MessageBox.Show(m.ErrorStr);
            }
            else
            {
                // определяем различные цвета
                var colorAlpha = 0.5f;
                var material = blueOnlyCheckBox.IsChecked == true
                    ? PhongMaterials.Blue
                    : materials[rnd.Next(0, materials.Count - 1)];
                material.DiffuseColor = new Color4(material.DiffuseColor.ToVector3(), colorAlpha);
                model.Material = material;
                //model.CullMode = CullMode.Back;
                model.IsTransparent = true;
                viewport.Items.Add(model);
            }

            // визуализируем ребра
            var inxs2 = new IntCollection();
            m.Edges.ForEach(x2 => inxs2.AddAll(x2.Indices));
            m.Faces.ForEach(x => x.Edges.ForEach(x2 => inxs2.AddAll(x2.Indices)));
            LineGeometry3D lineGeom = new() { Positions = m.Positions, Indices = inxs2, };
            LineGeometryModel3D edge = new() { Geometry = lineGeom, Color = Colors.Black };
            viewport.Items.Add(edge);
        }

     
        private MeshGeometry3D ToGeometry(BaseModel m)
        {
            var model = new MeshGeometry3D()
            {
                Positions = m.Positions,
                Indices = m.Indices,
                Normals = m.Normals,
                TextureCoordinates = null,
                Tangents = null,
                BiTangents = null,

            };
            return model;
        }

        private OrthographicCamera _ortoCam = new OrthographicCamera() { Position = new Point3D(100, 100, 100), LookDirection = new Vector3D(-100, -100, -100), UpDirection = new Vector3D(0, 0, 1), Width = 200, FarPlaneDistance = 1000 };
        private PerspectiveCamera _perspCam = new PerspectiveCamera() { Position = new Point3D(100, 100, 100), LookDirection = new Vector3D(-100, -100, -100), UpDirection = new Vector3D(0, 0, 1) };

        private void IsPerspectiveCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            viewport.Camera = (bool)isPerspectiveCheckBox.IsChecked! ? _perspCam : _ortoCam;
        }


        (Vector3Collection, IntCollection) GetContourPointAndInx(BaseModel m)
        {
            Vector3Collection points = new();
            IntCollection inxs = new();
            if (m is Extrusion_with_angle ex)
            {
                points = new Vector3Collection(ex.points.Select(p => p.ToVector3(0)));
                inxs = GenerateCounturIndicies(points.Count);
            }
            if (m is Extrusion_with_hole ex2)
            {
                points = new Vector3Collection(ex2.points.Select(p => p.ToVector3(0)));
                inxs = GenerateCounturIndicies(points.Count);
            }
            if (m is HoleTest ex3)
            {
                points = new Vector3Collection(ex3.points.Select(p => p.ToVector3(0)));
                inxs = GenerateCounturIndicies(points.Count);
            }
            if (m is Extrusion_with_Revolved_along_line ex4)
            {
                points = new Vector3Collection(ex4.pointsFigure.Select(p => p.ToVector3(0)));
                inxs = GenerateCounturIndicies(points.Count);
                int endContourCount = points.Count;
                var linePts = ex4.pointsLine.Select(p => p.ToVector3(0)).ToList();
                var newLineInxs = GenerateCounturIndicies(points.Count, false);
                points.AddRange(linePts);
                inxs.AddRange(newLineInxs.Select(i => i + endContourCount));
            }

            return (points, inxs);
        }

        private IntCollection GenerateCounturIndicies(int pointsCount, bool isClosedContour=true)
        {
            var inxs = new IntCollection();

            for (int i = 0; i < pointsCount - 1; i++)
            {
                inxs.Add(i);
                inxs.Add(i + 1);
            }

            if (isClosedContour)
            {
                inxs.Add(pointsCount - 1);
                inxs.Add(0);
            }

            return inxs;
        }

    }




    /// <summary>
    /// Класс для установки прозрачности с помощью слайдера в объектах Viewport3DX
    /// </summary>
    class OpacityHelper : ObservableObject
    {
        private Viewport3DX view;
        public OpacityHelper(Viewport3DX view)
        {
            this.view = view;
        }

        private float _opacity = 1.0f;
        public float Opacity
        {
            get => _opacity;
            set
            {
                if (SetValue(ref _opacity, value))
                {
                    foreach (var item in view.Items)
                    {
                        var model = (item as MeshGeometryModel3D);
                        if (model?.Material is PhongMaterial m)
                        {
                            m.DiffuseColor = new Color4(m.DiffuseColor.ToVector3(), value);
                            model!.IsTransparent = value < 1;
                        }
                    }
                }
            }
        }

        private bool _showWireframe = false;
        public bool IsShowWireframe
        {
            get => _showWireframe;
            set
            {
                if (SetValue(ref _showWireframe, value))
                {
                    foreach (var item in view.Items)
                    {
                        var model = (item as MeshGeometryModel3D);
                        if (model != null)
                        {
                            model.RenderWireframe = _showWireframe;
                        }
                    }
                }
            }
        }

        private bool _showGrid = false;
        public bool IsShowGrid
        {
            get => _showGrid;
            set
            {
                if (SetValue(ref _showGrid, value))
                {
                    foreach (var item in view.Items)
                    {
                        if (item is LineGeometryModel3D model && model.Name.Contains("grid"))
                        {
                            model.Visibility = _showGrid ? Visibility.Visible : Visibility.Hidden;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Реализует поддержку извещения об изменении свойств
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string info = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        protected bool SetValue<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
        {
            if (object.Equals(backingField, value))
            {
                return false;
            }

            backingField = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
