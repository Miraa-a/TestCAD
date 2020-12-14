using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;

namespace TestCAD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private EffectsManager manager;
        private Viewport3DX viewport;
        private Models models = new Models();
        private SceneNodeGroupModel3D sceneNodeGroup;
        private bool flagLast = false;
        private bool flagAll = false;

        public MainWindow()
        {
            InitializeComponent();
            manager = new DefaultEffectsManager();
            buttonRemoveViewport.IsEnabled = false;
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e) //добавление куба на сцену
        {
            viewport.Items.Add(models.VisualFigure(0));
            var rmp = new UICompositeManipulator3D();
            rmp.Bind((GeometryModel3D)viewport.Items.Last());
            viewport.Items.Add(rmp);
        }
        private void Button_Click_Remove(object sender, RoutedEventArgs e) //удаление последней фигуры со сцены  со сцены
        {
            viewport.Items.RemoveAt(viewport.Items.Count - 1);
        }

        private void Button_Click_Initialize(object sender, RoutedEventArgs e) //инициализация окна вывода
        {
            viewport = new Viewport3DX();
            viewport.BackgroundColor = Colors.White;
            viewport.ShowCoordinateSystem = true;
            viewport.EffectsManager = manager;
            viewport.Items.Add(new DirectionalLight3D() { Direction = new System.Windows.Media.Media3D.Vector3D(-1, -1, -1) });
            viewport.Items.Add(new AmbientLight3D() { Color = Color.FromArgb(255, 50, 50, 50) });
            sceneNodeGroup = new SceneNodeGroupModel3D();
            viewport.Items.Add(sceneNodeGroup);
            //LineGeometry3D grid = LineBuilder.GenerateGrid(new Vector3(0, 1, 0), -5, 5, -5, 5);
            viewport.Items.Add(new AxisPlaneGridModel3D());
            mainGrid.Children.Add(viewport);

            buttonInit.IsEnabled = false;
            buttonRemoveViewport.IsEnabled = true;

        }


        private void Button_Click_AddSphere(object sender, RoutedEventArgs e) //добавление сферы на сцену
        {
            viewport.Items.Add(models.VisualFigure(1));
            var rmp = new UICompositeManipulator3D();
            rmp.Bind((GeometryModel3D)viewport.Items.Last());
            viewport.Items.Add(rmp);
            
        }
        private void Button_Click_AddCylinder(object sender, RoutedEventArgs e) //добавление цилиндра на сцену
        {
            viewport.Items.Add(models.VisualFigure(2));
            var rmp = new UICompositeManipulator3D();
            rmp.Bind((GeometryModel3D)viewport.Items.Last());
            viewport.Items.Add(rmp);
            
        }
        private void Button_Click_AddGlass(object sender, RoutedEventArgs e) //добавление чаши на сцену
        {
            viewport.Items.Add(models.VisualFigure(3));
            var rmp = new UICompositeManipulator3D();
            rmp.Bind((GeometryModel3D)viewport.Items.Last());
            viewport.Items.Add(rmp);
            
        }

        private void Button_Click_AddExtrusion(object sender, RoutedEventArgs e) //добавление выдавливания на сцену
        {
            viewport.Items.Add(models.VisualFigure(4));
            var rmp = new UICompositeManipulator3D();
            rmp.Bind((GeometryModel3D)viewport.Items.Last());
            viewport.Items.Add(rmp);
            
        }
        private void buttonSceneNode_Click(object sender, RoutedEventArgs e) //вывод рандомной фигуры через SceneNode
        {           
            sceneNodeGroup.AddNode(models.GetSceneNodeRandom());
            var rmp = new UICompositeManipulator3D();
            rmp.Bind((GeometryModel3D)viewport.Items.Last());
            viewport.Items.Add(rmp);
            
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.Children.Remove(viewport);
            buttonInit.IsEnabled = true;
            buttonRemoveViewport.IsEnabled = false;
        }

    }

    public class Models
    {
        private IList<Geometry3D> models { get; } = new List<Geometry3D>();
        private PhongMaterialCollection materials = new PhongMaterialCollection();
        private Random rnd = new Random();
        public Models()
        {
            BaseModel m = new Box_Model(); 
            m.Update();
            models.Add(ToGeometry(m));
            
            m = new Sphere_Model();
            m.Update();
            models.Add(ToGeometry(m));

            m = new Cylinder_Model();
            m.Update();
            models.Add(ToGeometry(m));

            m = new Revolved();
            m.Update();
            models.Add(ToGeometry(m));

            m = new Extrusion();
            m.Update();
            models.Add(ToGeometry(m));

        }
        private Geometry3D ToGeometry(BaseModel m)
        {
            Geometry3D model = new MeshGeometry3D()
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

        public MeshGeometryModel3D VisualFigure(int idx)
        {
            MeshGeometryModel3D model = new MeshGeometryModel3D() { Geometry = models[idx], CullMode = SharpDX.Direct3D11.CullMode.Back };
            var scale = new System.Windows.Media.Media3D.ScaleTransform3D(5, 5, 5);
            var translate = new System.Windows.Media.Media3D.TranslateTransform3D(rnd.NextDouble(-20, 20), rnd.NextDouble(0, 15), rnd.NextDouble(-20, 20));
            var group = new System.Windows.Media.Media3D.Transform3DGroup();
            group.Children.Add(scale);
            group.Children.Add(translate);
            model.Transform = group;
            var material = materials[rnd.Next(0, materials.Count - 1)];
            model.Material = material;
            return model;
        }
        //public MeshGeometryModel3D GetModelRandom()
        //{
        //    var idx = rnd.Next(0, models.Count);
        //    MeshGeometryModel3D model = new MeshGeometryModel3D() { Geometry = models[idx], CullMode = SharpDX.Direct3D11.CullMode.Back };
        //    var scale = new System.Windows.Media.Media3D.ScaleTransform3D(/*rnd.NextDouble(1, 5)*/ 5, /*rnd.NextDouble(1, 5)*/ 5 , /*rnd.NextDouble(1, 5)*/ 5);
        //    var translate = new System.Windows.Media.Media3D.TranslateTransform3D(rnd.NextDouble(-20, 20), rnd.NextDouble(-20, 20), rnd.NextDouble(-20, 20));
        //    var group = new System.Windows.Media.Media3D.Transform3DGroup();
        //    group.Children.Add(scale);
        //    group.Children.Add(translate);
        //    model.Transform = group;
        //    var material = materials[rnd.Next(0, materials.Count - 1)];
        //    model.Material = material;
        //    if (material.DiffuseColor.Alpha < 1)
        //    {
        //        model.IsTransparent = true;
        //    }
        //    return model;
        //}

        public MeshNode GetSceneNodeRandom()
        {
            var idx = rnd.Next(0, models.Count);
            MeshNode model = new MeshNode() { Geometry = models[idx], CullMode = SharpDX.Direct3D11.CullMode.Back };
            var scale = SharpDX.Matrix.Scaling((float)rnd.NextDouble(1, 5), (float)rnd.NextDouble(1, 5), (float)rnd.NextDouble(1, 5));
            var translate = SharpDX.Matrix.Translation((float)rnd.NextDouble(-20, 20), (float)rnd.NextDouble(0, 15), (float)rnd.NextDouble(-20, 20));
            model.ModelMatrix = scale * translate;
            var material = materials[rnd.Next(0, materials.Count - 1)];
            model.Material = material;
            if (material.DiffuseColor.Alpha < 1)
            {
                model.IsTransparent = true;
            }
            return model;
        }

    }
}
