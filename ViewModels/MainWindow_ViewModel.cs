using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using SharpDX;

using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Assimp;

using Newtonsoft.Json;

namespace NMSTools.Builder.ViewModels
{
    using NMSTools.Framework.Converters;
    using NMSTools.Framework.Models;

    using Base;
    using Commands;
    using Primitives;

    using Pnt3 = System.Windows.Media.Media3D.Point3D;
    using Vec3 = System.Windows.Media.Media3D.Vector3D;

    using Color = System.Windows.Media.Color;
    using Colors = System.Windows.Media.Colors;

    using Transform3D = System.Windows.Media.Media3D.TranslateTransform3D;

    public class MainWindow_ViewModel : ViewModel
    {
        private const string selectionEffectString = "highlight[color:#0064A6]";

        private readonly IEffectsManager effectManager;
        private readonly Camera camera;

        public static readonly DependencyProperty AssemblyVersionProperty;

        public static readonly DependencyProperty AmbientLightColorProperty;
        public static readonly DependencyProperty DirectionalLightColorProperty;
        public static readonly DependencyProperty DirectionalLightDirectionProperty;
        public static readonly DependencyProperty BuildingItmeCollectionProperty;
        public static readonly DependencyProperty SelectedObjectProperty;

        public string AssemblyVersion
        {
            get { return (string)GetValue(AssemblyVersionProperty); }
            set { SetValue(AssemblyVersionProperty, value); }
        }

        public Color AmbientLightColor
        {
            get => (Color)GetValue(AmbientLightColorProperty);
            set => SetValue(AmbientLightColorProperty, value);
        }

        public Color DirectionalLightColor
        {
            get => (Color)GetValue(DirectionalLightColorProperty);
            set => SetValue(DirectionalLightColorProperty, value);
        }

        public Vec3 DirectionalLighDirection
        {
            get => (Vec3)GetValue(DirectionalLightDirectionProperty);
            set => SetValue(DirectionalLightDirectionProperty, value);
        }

        public ObservableCollection<Shape> BuildingItmeCollection
        {
            get => (ObservableCollection<Shape>)GetValue(BuildingItmeCollectionProperty);
            set => SetValue(BuildingItmeCollectionProperty, value);
        }

        public MeshGeometryModel3D SelectedObject
        {
            get => (MeshGeometryModel3D)GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        public IEffectsManager EffectsManager => effectManager;
        public Camera Camera => camera;

        public ICommand TestCommand { get; }
        public ICommand MouseDownCommand { get; }

        static MainWindow_ViewModel()
        {
            AssemblyVersionProperty = DependencyProperty.Register("AssemblyVersion",
                typeof(string), typeof(MainWindow_ViewModel),
                new FrameworkPropertyMetadata(string.Empty));

            AmbientLightColorProperty = DependencyProperty.Register("AmbientLightColor",
                typeof(Color), typeof(MainWindow_ViewModel),
                new PropertyMetadata(Colors.DimGray));

            DirectionalLightColorProperty = DependencyProperty.Register("DirectionalLightColor",
                typeof(Color), typeof(MainWindow_ViewModel),
                new PropertyMetadata(Colors.White));

            DirectionalLightDirectionProperty = DependencyProperty.Register("DirectionalLighDirection",
                typeof(Vec3), typeof(MainWindow_ViewModel),
                new PropertyMetadata(new Vec3(-2, -5, -2)));

            BuildingItmeCollectionProperty = DependencyProperty.Register("BuildingItmeCollection",
                typeof(ObservableCollection<Shape>), typeof(MainWindow_ViewModel),
                new PropertyMetadata(new ObservableCollection<Shape>()));

            SelectedObjectProperty = DependencyProperty.Register("SelectedObject",
                typeof(MeshGeometryModel3D), typeof(MainWindow_ViewModel),
                new PropertyMetadata(null));
        }

        public MainWindow_ViewModel()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            AssemblyVersion = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);

            effectManager = new DefaultEffectsManager();
            camera = new PerspectiveCamera
            {
                Position = new Pnt3(50, 50, 50),
                LookDirection = new Vec3(-50, -50, -50),
                UpDirection = new Vec3(0, 1, 0),
                NearPlaneDistance = 1,
                FarPlaneDistance = 2000
            };

            MouseDownCommand = new Command(i =>
            {
                if (!(i is MouseDown3DEventArgs e))
                    return;

                if (e.HitTestResult == null)
                    return;

                if (!e.HitTestResult.IsValid)
                    return;

                if (e.HitTestResult.ModelHit == null)
                    return;

                if (e.HitTestResult.ModelHit is MeshGeometryModel3D m)
                {
                    if (SelectedObject != null)
                        SelectedObject.PostEffects = string.Empty;

                    SelectedObject = m;
                    SelectedObject.PostEffects = selectionEffectString;
                    return;
                }

            });

            TestCommand = new Command(i =>
            {
                Task.Run(() =>
                {
                    using var loader = new Importer();
                    return loader.Load("C:\\Users\\dhr\\Desktop\\CONCRETE_WALL.");
                })
                .ContinueWith(n =>
                {
                    var model = n.Result;

                }, TaskScheduler.FromCurrentSynchronizationContext());

            });

            Task.Run(() => Deserialize<NMSRoot>("D:\\NMSProjects\\Archives\\Misc\\save.hg"))
            .ContinueWith(i =>
            {
                foreach (var playerBase in i.Result.PlayerStateData.PersistentPlayerBases)
                {
                    if (!playerBase.Name.Equals("Test Base"))
                        continue;

                    foreach (var baseItem in playerBase.Objects)
                    {
                        Shape itemShape = new Shape();

                        var b1 = new MeshBuilder();
                        b1.AddBox(new Vector3(0, 0, 0), 1, 1, 1);

                        itemShape.Geometry = b1.ToMeshGeometry3D();
                        itemShape.Translation = new Transform3D(baseItem.Position.X, baseItem.Position.Y, baseItem.Position.Z);
                        itemShape.Rotation = new Transform3D(baseItem.At.X, baseItem.At.Y, baseItem.At.Z);
                        itemShape.Scale = new Transform3D(baseItem.Up.X, baseItem.Up.Y, baseItem.Up.Z);
                        itemShape.Material = PhongMaterials.Blue;
                        itemShape.UpdateTransform();

                        BuildingItmeCollection.Add(itemShape);
                    }

                    break;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());


        }

        static T Deserialize<T>(string filename) where T : class
        {
            T result = default;
            FileStream inputFile = default;
            StreamReader sr = default;
            JsonTextReader jtr = default;
            JsonSerializer serializer = default;

            try
            {
                serializer = new JsonSerializer();
                serializer.Converters.Add(new DoubleConverter());
                serializer.Converters.Add(new Vector3Converter());
                serializer.Converters.Add(new Vector4Converter());
                serializer.Error += Serializer_Error;

                inputFile = File.Open(filename, FileMode.Open);
                sr = new StreamReader(inputFile);
                jtr = new JsonTextReader(sr);

                result = serializer.Deserialize<T>(jtr);
            }
            finally
            {
                if (jtr != null)
                {
                    jtr.Close();

                    ((IDisposable)jtr).Dispose();
                    jtr = null;
                }

                if (sr != null)
                {
                    sr.Close();

                    sr.Dispose();
                    sr = null;
                }

                if (inputFile != null)
                {
                    inputFile.Close();

                    inputFile.Dispose();
                    inputFile = null;
                }

                serializer.Error -= Serializer_Error;
                serializer = null;
            }

            return result;
        }

        private static void Serializer_Error(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e) => Console.WriteLine(e.ErrorContext.Error.Message);


        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                //cts.Cancel(true);
                //compositeHelper.Dispose();

                if (EffectsManager != null)
                {
                    var effectManager = EffectsManager as IDisposable;
                    Disposer.RemoveAndDispose(ref effectManager);
                }

                disposedValue = true;
                GC.SuppressFinalize(this);
            }
        }

        ~MainWindow_ViewModel() => Dispose(false);

        public override void Dispose() => Dispose(true);
        #endregion
    }
}
