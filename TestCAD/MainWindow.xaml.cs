using SharpDX;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Media3D;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using SharpDX.Direct3D11;
using TestCAD.Models;
using Colors = System.Windows.Media.Colors;
using MeshGeometry3D = HelixToolkit.SharpDX.Core.MeshGeometry3D;

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

            sceneNodeGroup = new SceneNodeGroupModel3D();
            viewport.Items.Add(sceneNodeGroup);
            viewport.Items.Add(new AxisPlaneGridModel3D());

            WindowState = WindowState.Maximized;

            var opacityHelper = new OpacityHelper(viewport);
            opacityTextPanel.DataContext = opacityHelper;
            opacitySlider.DataContext = opacityHelper;
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

        private void Button_Click_AddExtrusion(object sender, RoutedEventArgs e) //добавление выдавливания на сцену
        {
            VisualizeFigure(new Extrusion());
        }
        private void Button_Click_AddExtrusionAngel(object sender, RoutedEventArgs e) //добавление выдавливания на сцену
        {
            VisualizeFigure(new Extrusion_with_angle());
        }
        public void VisualizeFigure(BaseModel m)
        {
            // визуализируем фигуру
            m.Update();
            var tmp = ToGeometry(m);
            MeshGeometryModel3D model = new() { Geometry = tmp };

            var group = new Transform3DGroup();
            group.Children.Add(new ScaleTransform3D(5, 5, 5));
            group.Children.Add(new TranslateTransform3D(rnd.NextDouble(-20, 20), rnd.NextDouble(0, 15), rnd.NextDouble(-20, 20)));
            model.Transform = group;

            // определяем различные цвета
            var colorAlpha = 1.0f;
            var material = blueOnlyCheckBox.IsChecked == true ? PhongMaterials.Blue : materials[rnd.Next(0, materials.Count - 1)];
            material.DiffuseColor = new Color4(material.DiffuseColor.ToVector3(), colorAlpha);
            model.Material = material;
            //model.CullMode = CullMode.Back;
            model.IsTransparent = true;
            viewport.Items.Add(model);

            // визуализируем ребра
            var inxs2 = new IntCollection();
            m.Edges.ForEach(x2 => inxs2.AddAll(x2.Indices));
            //m.Faces.ForEach(x => x.Edges.ForEach(x2 => inxs2.AddAll(x2.Indices)));
            LineGeometry3D lineGeom = new() { Positions = m.Positions, Indices = inxs2, };
            LineGeometryModel3D edge = new() { Geometry = lineGeom, Color = Colors.Red, Transform = group };
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

        private float opacity = 1.0f;
        public float Opacity
        {
            get => opacity;
            set
            {
                if (SetValue(ref opacity, value))
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
