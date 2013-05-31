﻿//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using Dynamo.TypeSystem;
using Microsoft.FSharp.Collections;
using Dynamo.Connectors;
using Value = Dynamo.FScheme.Value;
using HelixToolkit.Wpf;
using Dynamo.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    [NodeName("Watch 3D")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Shows a dynamic preview of geometry.")]
    [AlsoKnownAs("Dynamo.Nodes.dyn3DPreview")]
    public class dynWatch3D : dynNodeWithOneOutput
    {
        WatchView _watchView;

        private PointsVisual3D _points;
        private LinesVisual3D _lines;
        private readonly List<MeshVisual3D> _meshes = new List<MeshVisual3D>();

        public Point3DCollection Points { get; set; }
        public Point3DCollection Lines { get; set; }
        public List<Mesh3D> Meshes { get; set; }

        List<Color> _colors = new List<Color>();
        
        private bool _requiresRedraw = false;
        private bool _isRendering = false;

        public dynWatch3D()
        {
            var t = new GuessType();
            InPortData.Add(new PortData("IN", "Incoming geometry objects.", t));
            OutPortData.Add(new PortData("OUT", "Watch contents, passed through", t));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override void SetupCustomUIElements(Controls.dynNodeView nodeUI)
        {
            var mi = new MenuItem { Header = "Zoom to Fit" };
            mi.Click += mi_Click;

            nodeUI.MainContextMenu.Items.Add(mi);

            //take out the left and right margins and make this so it's not so wide
            //NodeUI.inputGrid.Margin = new Thickness(10, 10, 10, 10);

            //add a 3D viewport to the input grid
            //http://helixtoolkit.codeplex.com/wikipage?title=HelixViewport3D&referringTitle=Documentation
            _watchView = new WatchView { watch_view = { DataContext = this } };

            RenderOptions.SetEdgeMode(_watchView, EdgeMode.Unspecified);

            Points = new Point3DCollection();
            Lines = new Point3DCollection();

            _points = new PointsVisual3D { Color = Colors.Red, Size = 6 };
            _lines = new LinesVisual3D { Color = Colors.Blue, Thickness = 1 };

            _points.Points = Points;
            _lines.Points = Lines;

            _watchView.watch_view.Children.Add(_lines);
            _watchView.watch_view.Children.Add(_points);

            _watchView.watch_view.Children.Add(new DefaultLights());

            _watchView.Width = 400;
            _watchView.Height = 300;

            var backgroundRect = new System.Windows.Shapes.Rectangle
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                RadiusX = 10,
                RadiusY = 10,
                IsHitTestVisible = false
            };
            var bc = new BrushConverter();
            var strokeBrush = (Brush)bc.ConvertFrom("#313131");
            backgroundRect.Stroke = strokeBrush;
            backgroundRect.StrokeThickness = 1;
            var backgroundBrush = new SolidColorBrush(Color.FromRgb(250, 250, 216));
            backgroundRect.Fill = backgroundBrush;
            nodeUI.inputGrid.Children.Add(backgroundRect);
            nodeUI.inputGrid.Children.Add(_watchView);

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            _watchView.watch_view.ZoomExtents();
        }

        private static void GetUpstreamIDrawable(List<IDrawable> drawables, Dictionary<int, Tuple<int, dynNodeModel>> inputs)
        {
            foreach (KeyValuePair<int, Tuple<int, dynNodeModel>> pair in inputs)
            {
                if (pair.Value == null)
                    continue;

                dynNodeModel node = pair.Value.Item2;
                var drawable = node as IDrawable;

                if (node.IsVisible && drawable != null)
                    drawables.Add(drawable);

                if (node.IsUpstreamVisible)
                    GetUpstreamIDrawable(drawables, node.Inputs);
                else
                    continue; // don't bother checking if function

                //if the node is function then get all the 
                //drawables inside that node. only do this if the
                //node's workspace is the home space to avoid infinite
                //recursion in the case of custom nodes in custom nodes
                if (node is dynFunction && node.WorkSpace == dynSettings.Controller.DynamoModel.HomeSpace)
                {
                    dynFunction func = (dynFunction)node;
                    IEnumerable<dynNodeModel> topElements = func.Definition.Workspace.GetTopMostNodes();
                    foreach (dynNodeModel innerNode in topElements)
                    {
                        GetUpstreamIDrawable(drawables, innerNode.Inputs);
                    }
                }
            }
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (_isRendering)
                return;

            if (!_requiresRedraw)
                return;

            _isRendering = true;

            Points = null;
            Lines = null;
            _lines.Points = null;
            _points.Points = null;

            Points = new Point3DCollection();
            Lines = new Point3DCollection();
            Meshes = new List<Mesh3D>();

            // a list of all the upstream IDrawable nodes
            var drawables = new List<IDrawable>();

            GetUpstreamIDrawable(drawables, Inputs);

            foreach (IDrawable d in drawables) 
            {
                d.Draw();

                foreach (Point3D p in d.RenderDescription.points)
                {
                    Points.Add(p);
                }

                foreach (Point3D p in d.RenderDescription.lines)
                {
                    Lines.Add(p);
                }

                foreach (Mesh3D mesh in d.RenderDescription.meshes)
                {
                    Meshes.Add(mesh);
                }
            }

            _lines.Points = Lines;
            _points.Points = Points;

            // remove old meshes from the renderer
            foreach (MeshVisual3D mesh in _meshes)
            {
                _watchView.watch_view.Children.Remove(mesh);
            }

            _meshes.Clear();

            foreach (MeshVisual3D vismesh in Meshes.Select(MakeMeshVisual3D)) 
            {
                _watchView.watch_view.Children.Add(vismesh);
                _meshes.Add(vismesh);
            }

            _requiresRedraw = false;
            _isRendering = false;
        }

        static MeshVisual3D MakeMeshVisual3D(Mesh3D mesh)
        {
            var vismesh = new MeshVisual3D
            {
                Content = new GeometryModel3D
                {
                    Geometry = mesh.ToMeshGeometry3D(), Material = Materials.White
                }
            };
            return vismesh;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            _requiresRedraw = true;

            return input;
        }
    }
}
