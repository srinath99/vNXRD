﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MapControl;

namespace vNXRD
{
    /// <summary>
    /// Displays web mercator map tiles.
    /// </summary>
    public class NexradTileLayer : MapTileLayerBase
    {
        public const int TileSize = 256;

        public static readonly Point MapTopLeft = new Point(
            -180d * MapProjection.Wgs84MetersPerDegree, 180d * MapProjection.Wgs84MetersPerDegree);

        /// <summary>
        /// A default MapTileLayer using OpenStreetMap data.
        /// </summary>
        public static NexradTileLayer OpenStreetMapTileLayer
        {
            get
            {
                return new NexradTileLayer
                {
                    SourceName = "OpenStreetMap",
                    Description = "© [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)",
                    TileSource = new TileSource { UriFormat = "https://tile.openstreetmap.org/{z}/{x}/{y}.png" },
                    MaxZoomLevel = 19
                };
            }
        }

        public static readonly DependencyProperty MinZoomLevelProperty = DependencyProperty.Register(
            nameof(MinZoomLevel), typeof(int), typeof(MapTileLayer), new PropertyMetadata(0));

        public static readonly DependencyProperty MaxZoomLevelProperty = DependencyProperty.Register(
            nameof(MaxZoomLevel), typeof(int), typeof(MapTileLayer), new PropertyMetadata(18));

        public NexradTileLayer()
            : this(new TileImageLoader())
        {
        }

        public NexradTileLayer(ITileImageLoader tileImageLoader)
            : base(tileImageLoader)
        {
        }

        public TileMatrix TileMatrix { get; private set; }

        public IReadOnlyCollection<Tile> Tiles { get; private set; } = new List<Tile>();

        /// <summary>
        /// Minimum zoom level supported by the MapTileLayer. Default value is 0.
        /// </summary>
        public int MinZoomLevel
        {
            get { return (int)GetValue(MinZoomLevelProperty); }
            set { SetValue(MinZoomLevelProperty, value); }
        }

        /// <summary>
        /// Maximum zoom level supported by the MapTileLayer. Default value is 18.
        /// </summary>
        public int MaxZoomLevel
        {
            get { return (int)GetValue(MaxZoomLevelProperty); }
            set { SetValue(MaxZoomLevelProperty, value); }
        }

        protected override void TileSourcePropertyChanged()
        {
            if (TileMatrix != null)
            {
                Tiles = new List<Tile>();
                UpdateTiles();
            }
        }

        protected override void UpdateTileLayer()
        {
            UpdateTimer.Stop();

            if (ParentMap == null || !ParentMap.MapProjection.IsWebMercator)
            {
                TileMatrix = null;
                UpdateTiles();
            }
            else if (SetTileMatrix())
            {
                SetRenderTransform();
                UpdateTiles();
            }
        }

        protected override void SetRenderTransform()
        {
            // tile matrix origin in pixels
            //
            var tileMatrixOrigin = new Point(TileSize * TileMatrix.XMin, TileSize * TileMatrix.YMin);

            var tileMatrixScale = ViewTransform.ZoomLevelToScale(TileMatrix.ZoomLevel);

            ((MatrixTransform)RenderTransform).Matrix =
                ParentMap.ViewTransform.GetTileLayerTransform(tileMatrixScale, MapTopLeft, tileMatrixOrigin);
        }

        private bool SetTileMatrix()
        {
            var tileMatrixZoomLevel = (int)Math.Floor(ParentMap.ZoomLevel + 0.001); // avoid rounding issues

            var tileMatrixScale = ViewTransform.ZoomLevelToScale(tileMatrixZoomLevel);

            // bounds in tile pixels from view size
            //
            var bounds = ParentMap.ViewTransform.GetTileMatrixBounds(tileMatrixScale, MapTopLeft, ParentMap.RenderSize);

            // tile column and row index bounds
            //
            var xMin = (int)Math.Floor(bounds.X / TileSize);
            var yMin = (int)Math.Floor(bounds.Y / TileSize);
            var xMax = (int)Math.Floor((bounds.X + bounds.Width) / TileSize);
            var yMax = (int)Math.Floor((bounds.Y + bounds.Height) / TileSize);

            if (TileMatrix != null &&
                TileMatrix.ZoomLevel == tileMatrixZoomLevel &&
                TileMatrix.XMin == xMin && TileMatrix.YMin == yMin &&
                TileMatrix.XMax == xMax && TileMatrix.YMax == yMax)
            {
                return false;
            }

            TileMatrix = new TileMatrix(tileMatrixZoomLevel, xMin, yMin, xMax, yMax);

            return true;
        }

        private void UpdateTiles()
        {
            var newTiles = new List<Tile>();

            if (ParentMap != null && TileMatrix != null && TileSource != null)
            {
                var maxZoomLevel = Math.Min(TileMatrix.ZoomLevel, MaxZoomLevel);

                if (maxZoomLevel >= MinZoomLevel)
                {

                    var tileSize = 1 << (TileMatrix.ZoomLevel - maxZoomLevel);
                    var x1 = (int)Math.Floor((double)TileMatrix.XMin / tileSize); // may be negative
                    var x2 = TileMatrix.XMax / tileSize;
                    var y1 = Math.Max(TileMatrix.YMin / tileSize, 0);
                    var y2 = Math.Min(TileMatrix.YMax / tileSize, (1 << maxZoomLevel) - 1);

                    for (var y = y1; y <= y2; y++)
                    {
                        for (var x = x1; x <= x2; x++)
                        {
                            var tile = Tiles.FirstOrDefault(t => t.ZoomLevel == maxZoomLevel && t.X == x && t.Y == y);

                            if (tile == null)
                            {
                                tile = new Tile(maxZoomLevel, x, y);

                                var equivalentTile = Tiles.FirstOrDefault(
                                    t => t.ZoomLevel == maxZoomLevel && t.XIndex == tile.XIndex && t.Y == y && t.Image.Source != null);

                                if (equivalentTile != null)
                                {
                                    tile.SetImage(equivalentTile.Image.Source, false); // no fade-in animation
                                }
                            }

                            newTiles.Add(tile);
                        }
                    }
                    
                }
            }

            Tiles = newTiles;

            Children.Clear();

            foreach (var tile in Tiles)
            {
                Children.Add(tile.Image);
            }

            TileImageLoader.LoadTilesAsync(Tiles, TileSource, SourceName);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (var tile in Tiles)
            {
                tile.Image.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (TileMatrix != null)
            {
                foreach (var tile in Tiles)
                {
                    var tileSize = TileSize << (TileMatrix.ZoomLevel - tile.ZoomLevel);
                    var x = tileSize * tile.X - TileSize * TileMatrix.XMin;
                    var y = tileSize * tile.Y - TileSize * TileMatrix.YMin;

                    tile.Image.Width = tileSize;
                    tile.Image.Height = tileSize;
                    tile.Image.Arrange(new Rect(x, y, tileSize, tileSize));
                }
            }

            return finalSize;
        }
    }
}