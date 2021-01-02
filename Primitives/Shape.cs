using System.Windows;

using HelixToolkit.Wpf.SharpDX;
using Transform3D = System.Windows.Media.Media3D.Transform3D;
using Transform3DGroup = System.Windows.Media.Media3D.Transform3DGroup;

namespace NMSTools.Builder.Primitives
{
    public class Shape : DependencyObject
    {
        public static readonly DependencyProperty GeometryProperty;
        public static readonly DependencyProperty TransformProperty;
        public static readonly DependencyProperty TranslationProperty;
        public static readonly DependencyProperty RotationProperty;
        public static readonly DependencyProperty ScaleProperty;
        public static readonly DependencyProperty MaterialProperty;

        static Shape()
        {
            GeometryProperty = DependencyProperty.Register("Geometry", typeof(Geometry3D), typeof(Shape), new PropertyMetadata(default));
            TransformProperty = DependencyProperty.Register("Transform", typeof(Transform3D), typeof(Shape), new PropertyMetadata(Transform3D.Identity));
            TranslationProperty = DependencyProperty.Register("Translation", typeof(Transform3D), typeof(Shape), new PropertyMetadata(Transform3D.Identity));
            RotationProperty = DependencyProperty.Register("Rotation", typeof(Transform3D), typeof(Shape), new PropertyMetadata(Transform3D.Identity));
            ScaleProperty = DependencyProperty.Register("Scale", typeof(Transform3D), typeof(Shape), new PropertyMetadata(Transform3D.Identity));
            MaterialProperty = DependencyProperty.Register("Material", typeof(Material), typeof(Shape), new PropertyMetadata(PhongMaterials.Blue));
        }

        public Geometry3D Geometry
        {
            get => (Geometry3D)GetValue(GeometryProperty);
            set => SetValue(GeometryProperty, value);
        }

        public Transform3D Transform
        {
            get => (Transform3D)GetValue(TransformProperty);
            private set => SetValue(TransformProperty, value);
        }

        public Transform3D Translation
        {
            get => (Transform3D)GetValue(TranslationProperty);
            set => SetValue(TranslationProperty, value);
        }

        public Transform3D Rotation
        {
            get => (Transform3D)GetValue(RotationProperty);
            set => SetValue(RotationProperty, value);
        }

        public Transform3D Scale
        {
            get => (Transform3D)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public Material Material
        {
            get => (Material)GetValue(MaterialProperty);
            set => SetValue(MaterialProperty, value);
        }

        public void UpdateTransform()
        {
            var transform = new Transform3DGroup();

            transform.Children.Add(Scale);
            transform.Children.Add(Rotation);
            transform.Children.Add(Translation);

            Transform = transform;
        }
    }
}
